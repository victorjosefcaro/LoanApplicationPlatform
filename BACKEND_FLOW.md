# Backend Feature Flow & Manual Test Guide

Developer companion to [`QA_TESTING_GUIDE.md`](QA_TESTING_GUIDE.md). Where the QA guide covers the happy-path E2E via Postman, this doc maps the **actual code paths, business-rule guards, and negative cases** so a developer can manually unit-test each backend feature.

- **Base URL (local):** `http://localhost:5131` — run with `dotnet run --project LoanApplicationPlatform.API`
- **Swagger:** `http://localhost:5131/swagger`
- All money values are PHP (`en-PH`). Interest is fixed server-side at **5%**.

---

## 1. Request Pipeline

```
HTTP → JWT Bearer auth → [Authorize]/Policy role gate → Controller
     → Service (business rules + status history) → Repository → EF Core
     → Global tenant query filter → SQL Server
```

- **JWT claims:** `NameIdentifier` (userId), `Role`, `tenant_id`. Issued in [AuthService.cs:47-52](LoanApplicationPlatform.API/Services/AuthService.cs#L47-L52), valid **2 hours**.
- **Multi-tenancy is implicit.** [TenantService](LoanApplicationPlatform.API/Services/TenantService.cs) reads `tenant_id` from the token; every entity has a global filter `TenantId == GetCurrentTenantId()`. You never send a tenant in the body. On insert, [context SaveChangesAsync](LoanApplicationPlatform.API/DbContexts/LoanApplicationPlatformContext.cs#L190-L206) stamps `TenantId` automatically.
- **Login is the only cross-tenant lookup** (`ignoreQueryFilters: true`), because the user isn't authenticated yet. Login still requires a `tenantId` and validates it against the user's own `TenantId`; a mismatch is rejected with the generic `401 "Invalid username or password."` (prevents tenant enumeration).

---

## 2. Roles & Policy Gates

| Role | Permissions |
|---|---|
| **Applicant** | create, update (Returned only), cancel, submit payments, view own apps |
| **Reviewer** | `/review` — policy `RequireReviewerRole` = Reviewer **or Admin** |
| **Approver** | `/approve` — policy `RequireApproverRole` = Approver **or Admin** |
| **Admin** | `/release`, post payments, treasury deposit/transactions, admin register |

Admin is folded into the Reviewer/Approver policies ([Program.cs:68-69](LoanApplicationPlatform.API/Program.cs#L68-L69)) — an Admin token can also review/approve.

### Seeded accounts (all password `password123`)

| Username | Role | Tenant |
|---|---|---|
| `t1_admin` / `t1_applicant` / `t1_reviewer` / `t1_approver` | Admin / Applicant / Reviewer / Approver | 1 |
| `t2_admin` / `t2_applicant` / `t2_reviewer` / `t2_approver` | Admin / Applicant / Reviewer / Approver | 2 |

Treasury seed balances: **Tenant 1 = ₱1,000,000**, **Tenant 2 = ₱2,000,000**.

---

## 3. Loan Lifecycle State Machine

`LoanStatus`: `Submitted, Returned, Reviewed, Approved, Rejected, Released, Cancelled, Completed`

```
Applicant POST ──► Submitted ──review──► Reviewed ──approve──► Approved ──release──► Released
                     │  ▲                   │                     │                     │
              (cancel)  └──(PUT update)──Returned◄──(return)──────┘              (all schedules Paid)
                     ▼         ▲              │                                          ▼
                 Cancelled     └──────────────┘                                     Completed
                                     Rejected (terminal, from review or approve)
```

Guards per transition (all in [LoanApplicationService.cs](LoanApplicationPlatform.API/Services/LoanApplicationService.cs)):

| Transition | Endpoint | Precondition |
|---|---|---|
| → Submitted | `POST /api/loanapplications` | `Amount/Term ≤ MonthlyIncome`; owner = caller |
| Returned → Submitted | `PUT /api/loanapplications/{id}` | status **Returned**; owner only; income re-check |
| → Cancelled | `PATCH /{id}/cancel` | status ∈ {Returned, Submitted}; owner only |
| Submitted → Reviewed/Returned/Rejected | `PATCH /{id}/review` | status **Submitted** |
| Reviewed → Approved/Rejected/Returned | `PATCH /{id}/approve` | status **Reviewed** |
| Approved → Released | `POST /{id}/release` | status **Approved**; treasury ≥ amount |
| Released → Completed | (auto on final payment post) | all schedules `Paid` |

Every transition writes a `LoanApplicationStatusHistory` row (prev status, new status, actor, remarks, timestamp), surfaced at `GET /{id}/history`.

`PaymentStatus`: `Pending, PaymentSubmitted, PartiallyPaid, Paid`. Payment is **two-phase**: applicant submits an amount → admin posts the **exact same** amount.

---

## 4. Setup for Manual Testing

The examples below use `curl` (works in PowerShell and bash). Save tokens to a shell variable.

### PowerShell

```powershell
# Login helper — captures the bare JWT string
function Get-Token($user) {
  $body = @{ username = $user; password = "password123" } | ConvertTo-Json
  (Invoke-RestMethod -Uri "http://localhost:5131/api/authentication/login" `
     -Method Post -ContentType "application/json" -Body $body)
}

$applicant = Get-Token "t1_applicant"
$reviewer  = Get-Token "t1_reviewer"
$approver  = Get-Token "t1_approver"
$admin     = Get-Token "t1_admin"

# Example authenticated call
Invoke-RestMethod -Uri "http://localhost:5131/api/loanapplications" `
  -Headers @{ Authorization = "Bearer $applicant" }
```

### bash / curl

```bash
BASE=http://localhost:5131
login() { curl -s -X POST $BASE/api/authentication/login \
  -H 'Content-Type: application/json' \
  -d "{\"username\":\"$1\",\"password\":\"password123\"}" | tr -d '"'; }

APPLICANT=$(login t1_applicant)
REVIEWER=$(login t1_reviewer)
APPROVER=$(login t1_approver)
ADMIN=$(login t1_admin)
```

---

## 5. Happy-Path E2E (matches QA guide §5)

```bash
# 1. Applicant creates → Submitted (returns id)
curl -s -X POST $BASE/api/loanapplications \
  -H "Authorization: Bearer $APPLICANT" -H 'Content-Type: application/json' \
  -d '{"applicantName":"Juan Dela Cruz","amount":120000,"termInMonths":12,"monthlyIncome":50000,"purpose":"Business capital"}'
# → 201 Created

# 2. Reviewer → Reviewed
curl -s -X PATCH $BASE/api/loanapplications/1/review \
  -H "Authorization: Bearer $REVIEWER" -H 'Content-Type: application/json' \
  -d '{"status":"Reviewed","remarks":"Looks good"}'          # → 204

# 3. Approver → Approved
curl -s -X PATCH $BASE/api/loanapplications/1/approve \
  -H "Authorization: Bearer $APPROVER" -H 'Content-Type: application/json' \
  -d '{"status":"Approved","remarks":"Approved"}'            # → 204

# 4. Admin releases funds → Released (generates 12 payment schedules)
curl -s -X POST $BASE/api/loanapplications/1/release \
  -H "Authorization: Bearer $ADMIN"                          # → 204

# 5. View schedules (grab a scheduleId)
curl -s $BASE/api/loanapplications/1/payments \
  -H "Authorization: Bearer $APPLICANT"

# 6. Applicant submits payment (amount optional; defaults to full remaining)
curl -s -X POST $BASE/api/loanapplications/1/payments/1/submit \
  -H "Authorization: Bearer $APPLICANT" -H 'Content-Type: application/json' \
  -d '{"amount":10500}'                                      # → 204

# 7. Admin posts the SAME amount
curl -s -X POST $BASE/api/loanapplications/1/payments/1/post \
  -H "Authorization: Bearer $ADMIN" -H 'Content-Type: application/json' \
  -d '{"amount":10500}'                                      # → 204

# 8. History
curl -s $BASE/api/loanapplications/1/history -H "Authorization: Bearer $APPLICANT"
```

> Amortized monthly = `(Amount + Amount×0.05) / Term`. For ₱120,000 over 12 months = `126000/12 = ₱10,500`.

---

## 6. Negative / Guard Test Matrix

The highest-value manual unit tests. Each row is one case that should **fail** with the given status. `Actual Result` and `Status` are left blank for the tester to fill in during a run. Steps beyond the sign-in flows assume the role tokens from §4 (`$APPLICANT`, `$REVIEWER`, `$APPROVER`, `$ADMIN`).

### Auth & Tenancy

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-001 | Login with wrong password | User should be logged out | 1. Open Loanly Web App<br>2. Enter a username<br>3. Enter a wrong password | 401 Unauthorized<br><br>Should display an error message "Invalid password. Please try again" | | |
| UT-002 | Login with empty username/password | User should be logged out | 1. Open Loanly Web App<br>2. Enter with an empty username/password | 400 Bad Request<br><br>Should display an error message "Username and password are required" | | |
| UT-003 | Register duplicate username (e.g. t1_applicant) via Register | User should be logged out | 1. Open Loanly Web App<br>2. When you're in Sign In, select "Create an account"<br>3. Enter a seeded username (e.g. t1_applicant)<br>4. Create and confirm a password | 409 Conflict<br><br>Should display an error message "Username is already taken" | | |
| UT-004 | Register with a nonexistent tenant | User should be logged out | 1. `POST /api/authentication/register`<br>2. Body `{"username":"newuser","password":"password123","tenantId":999}` | 400 Bad Request<br><br>"The selected tenant does not exist." | | |
| UT-005 | Applicant calls admin-only register | Logged in as Applicant (t1_applicant) | 1. `POST /api/authentication/admin/register` with `Authorization: Bearer $APPLICANT`<br>2. Body `{"username":"x","password":"password123","role":"Reviewer","tenantId":1}` | 403 Forbidden | | |
| UT-006 | Missing auth header on a protected endpoint | No token / logged out | 1. `GET /api/loanapplications` with no or blank `Authorization` header | 401 Unauthorized | | |
| UT-007 | Cross-tenant loan lookup | Logged in as a Tenant 1 user; a Tenant 2 loan id exists | 1. `GET /api/loanapplications/{t2id}` with `Authorization: Bearer` (Tenant 1 token) | 404 Not Found (row filtered out by tenant) | | |

### Creation validation ([LoanApplicationService.cs:65-72](LoanApplicationPlatform.API/Services/LoanApplicationService.cs#L65-L72))

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-008 | Create loan with insufficient income | Logged in as Applicant (t1_applicant) | 1. `POST /api/loanapplications`<br>2. Body `{"applicantName":"Test","amount":120000,"termInMonths":12,"monthlyIncome":5000,"purpose":"x"}` (10,500/mo due > 5,000 income) | 400 Bad Request<br><br>"Submission rejected: Your monthly income ... is insufficient ..." | | |
| UT-009 | Create loan with amount 0 | Logged in as Applicant | 1. `POST /api/loanapplications` with `amount:0` (other fields valid) | 400 Bad Request (Range: amount > 0) | | |
| UT-010 | Create loan with out-of-range term | Logged in as Applicant | 1. `POST /api/loanapplications` with `termInMonths:0` (repeat with `361`) | 400 Bad Request (Range 1–360) | | |
| UT-011 | Create loan with income 0 | Logged in as Applicant | 1. `POST /api/loanapplications` with `monthlyIncome:0` | 400 Bad Request (Range > 0) | | |
| UT-012 | Create loan with missing required fields | Logged in as Applicant | 1. `POST /api/loanapplications` omitting `purpose` and/or `applicantName` | 400 Bad Request (Required) | | |

### Transition guards (state machine)

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-013 | Review a non-Submitted application | App in `Reviewed` or `Approved` | 1. `PATCH /api/loanapplications/{id}/review` as Reviewer | 400 Bad Request<br><br>"Can only review submitted applications." | | |
| UT-014 | Approve a not-yet-Reviewed application | App in `Submitted` | 1. `PATCH /api/loanapplications/{id}/approve` as Approver | 400 Bad Request<br><br>"Can only process applications that have been reviewed." | | |
| UT-015 | Release a non-Approved application | App not in `Approved` | 1. `POST /api/loanapplications/{id}/release` as Admin | 400 Bad Request<br><br>"Can only release funds for approved applications." | | |
| UT-016 | Release when amount exceeds treasury | App `Approved`; amount > tenant treasury balance | 1. `POST /api/loanapplications/{id}/release` as Admin | 400 Bad Request<br><br>"Insufficient treasury funds..." | | |
| UT-017 | Update/resubmit an app not in Returned | App in `Submitted` (not `Returned`) | 1. `PUT /api/loanapplications/{id}` as owner Applicant | 400 Bad Request<br><br>"Can only update and resubmit applications in Returned status." | | |
| UT-018 | Cancel outside the cancel window | App in `Reviewed` or `Approved` | 1. `PATCH /api/loanapplications/{id}/cancel` as owner Applicant | 400 Bad Request<br><br>"Application cannot be cancelled at this stage." | | |
| UT-019 | Review with an invalid target status | App in `Submitted` | 1. `PATCH /api/loanapplications/{id}/review`<br>2. Body `{"status":"Approved"}` | 400 Bad Request (allowed: Returned \| Reviewed \| Rejected) | | |

### Ownership ([LoanApplicationService.cs:55-58](LoanApplicationPlatform.API/Services/LoanApplicationService.cs#L55-L58))

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-020 | Read another applicant's loan (same tenant) | Applicant A logged in; loan owned by Applicant B, same tenant | 1. `GET /api/loanapplications/{B_loan_id}` as Applicant A | 403 Forbidden | | |
| UT-021 | Mutate another applicant's loan (same tenant) | Applicant A logged in; loan owned by Applicant B, same tenant | 1. `PUT` / `cancel` / `submit` on `{B_loan_id}` as Applicant A | 403 Forbidden | | |

> Note the contrast: **same-tenant** ownership violation = `403`; **cross-tenant** = `404` (row is filtered out, so it looks not-found).

### Payment guards ([PaymentService.cs](LoanApplicationPlatform.API/Services/PaymentService.cs))

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-022 | Post a schedule that has no submitted payment | Released loan; schedule `Pending` | 1. Admin `POST /{id}/payments/{scheduleId}/post` (no prior submit) | 400 Bad Request<br><br>"Schedule must have a submitted payment to post." | | |
| UT-023 | Post an amount that mismatches the submission | Applicant submitted 10,500 on the schedule | 1. Admin `POST /{id}/payments/{scheduleId}/post` with `{"amount":9000}` | 400 Bad Request<br><br>"Posted payment amount must match..." | | |
| UT-024 | Submit on an already-paid schedule | Schedule already `Paid` | 1. Applicant `POST /{id}/payments/{scheduleId}/submit` | 400 Bad Request<br><br>"This schedule is already paid." | | |
| UT-025 | Submit more than the remaining balance | Remaining on schedule = 10,500 | 1. Applicant `POST /{id}/payments/{scheduleId}/submit` with `{"amount":20000}` | 400 Bad Request<br><br>"exceeds the remaining balance" | | |
| UT-026 | Submit a zero or negative amount | Released loan; schedule not paid | 1. Applicant `POST /{id}/payments/{scheduleId}/submit` with `{"amount":0}` (and negative) | 400 Bad Request<br><br>"must be greater than zero" | | |
| UT-027 | Submit a payment on a non-owned loan | Applicant is not the loan owner | 1. Non-owner Applicant `POST /{id}/payments/{scheduleId}/submit` | 403 Forbidden | | |
| UT-028 | Loan auto-completes after every schedule is paid | Released loan; all but the last schedule already `Paid` | 1. Submit then post the exact amount for the final schedule<br>2. `GET /{id}/history` | 204 No Content; loan auto-transitions to `Completed` (visible in `/history`) | | |

### Treasury ([TreasuryController.cs](LoanApplicationPlatform.API/Controllers/TreasuryController.cs))

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-029 | Applicant reads treasury balance | Logged in as Applicant | 1. `GET /api/treasury/balance` as Applicant | 403 Forbidden (Admin/Approver/Reviewer only) | | |
| UT-030 | Non-admin deposits to treasury | Logged in as a non-Admin role | 1. `POST /api/treasury/deposit` as non-Admin | 403 Forbidden | | |
| UT-031 | Admin deposits a zero/negative amount | Logged in as Admin | 1. `POST /api/treasury/deposit` with `{"amount":0}` (and negative) | 400 Bad Request | | |
| UT-032 | Admin reads treasury transactions | Logged in as Admin | 1. `GET /api/treasury/transactions` as Admin | 200 OK + `X-Pagination` header | | |

### Pagination (QA guide §6)

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-033 | Loan list returns pagination metadata | Authenticated; seeded loans exist | 1. `GET /api/loanapplications` | 200 OK; response has `X-Pagination` header (JSON metadata) | | |
| UT-034 | Treasury transactions return pagination metadata | Logged in as Admin; transactions exist | 1. `GET /api/treasury/transactions` | 200 OK; response has `X-Pagination` header | | |

---

## 7. Request Body Reference

| Endpoint | Body |
|---|---|
| `POST /authentication/login` | `{ "username", "password", "tenantId"? }` |
| `POST /authentication/register` | `{ "username", "password", "tenantId"? }` (defaults tenant 1) |
| `POST /authentication/admin/register` | `{ "username", "password", "role", "tenantId"? }` role ∈ Applicant/Reviewer/Approver/Admin |
| `POST /loanapplications` | `{ "applicantName", "amount", "termInMonths", "monthlyIncome", "purpose" }` |
| `PUT /loanapplications/{id}` | same as create |
| `PATCH /{id}/review` | `{ "status": "Reviewed\|Returned\|Rejected", "remarks"? }` |
| `PATCH /{id}/approve` | `{ "status": "Approved\|Rejected\|Returned", "remarks"? }` |
| `POST /{id}/release` | *(no body)* |
| `POST /{id}/payments/{scheduleId}/submit` | `{ "amount" }` (optional — omit for full remaining) |
| `POST /{id}/payments/{scheduleId}/post` | `{ "amount" }` (must equal submitted) |
| `POST /treasury/deposit` | `{ "amount" }` |

---

## 8. Notes & Gotchas

1. **QA guide is stale on usernames** — it references `acme_applicant`; the real seed is `t2_applicant`.
2. **`/cancel` is undocumented** in the QA guide but exists (Applicant-only, Submitted/Returned only).
3. **Resubmit only works from `Returned`** — a reviewer/approver must return an app before the applicant can `PUT` and resubmit; a fresh `Submitted` app cannot be edited.
4. **Admin can review & approve** (policies include Admin), broader than the QA guide's strict per-role matrix.
5. **Interest is hardcoded 5%** at creation — not a request field.
6. **Release & post are transactional** with a SQL `sp_getapplock` exclusive lock on the treasury to serialize concurrent money operations.

---

## 9. Alternate Flows (Non-Happy Paths)

The happy path (§5) is the ideal route where every stage advances. These are the other **legitimate** routes through the state machine — rework loops, terminal branches, and multi-installment payments. Each should be exercised on its own.

```
                        ┌─────────────── rework loop ───────────────┐
                        │                                            │
 POST ──► Submitted ──review──► Reviewed ──approve──► Approved ──release──► Released ──(all Paid)──► Completed
            │  │  ▲         │       │  │                  │  ▲                                          
   (cancel) │  │  └─Returned┤       │  └─Returned─────────┘  │(insufficient funds → stays Approved)     
            ▼  │            ▲       ▼                        ▼                                          
        Cancelled│  (PUT: applicant edits & resubmits)   Rejected (terminal)                           
                 └─► Rejected (terminal)                                                                
```

Each alternate flow is one end-to-end case. `Actual Result` and `Status` are left blank for the tester to fill in during a run. Steps assume the role tokens from §4.

| ID | Test Case | Preconditions | Test Steps | Expected Result | Actual Result | Status |
|---|---|---|---|---|---|---|
| UT-035 | Path A — Reviewer return & resubmit loop | Loan in `Submitted` (id from happy path); reviewer & owner-applicant tokens | 1. Reviewer `PATCH /{id}/review` with `{"status":"Returned","remarks":"Attach proof of income"}`<br>2. Applicant `PUT /{id}` with a revised body (allowed only from Returned)<br>3. Repeat steps 1–2 a second time | 204 on each call; app returns to `Submitted`; `/history` shows `Submitted→Returned→Submitted` with the correct actor per row. Loop is unbounded — repeat any number of times. | | |
| UT-036 | Path B — Approver return bounces to the start | Loan in `Reviewed`; approver, reviewer & applicant tokens | 1. Approver `PATCH /{id}/approve` with `{"status":"Returned","remarks":"Reviewer to re-verify"}`<br>2. Applicant `PUT /{id}` to edit & resubmit<br>3. Approver `PATCH /{id}/approve` again | 204 on return; after resubmit status is `Submitted`; step 3 fails with 400 (not `Reviewed`) until a reviewer advances it again. | | |
| UT-037 | Path C — Rejection is terminal | Loan in `Submitted` (or `Reviewed`) | 1. Reviewer (or Approver) `PATCH /review` (or `/approve`) with `{"status":"Rejected","remarks":"Ineligible"}`<br>2. Attempt `PUT`, `cancel`, `review`, `approve` on the same loan | 204, status `Rejected` (terminal); every subsequent transition fails with 400 (`PUT` not Returned, `cancel` not Submitted/Returned, `review`/`approve` wrong prior status). Record is frozen. | | |
| UT-038 | Path D — Cancellation window | Separate loans available in `Submitted`, `Returned`, and `Reviewed`/`Approved`/`Released` | 1. Applicant `PATCH /{id}/cancel` on a `Submitted` loan<br>2. Repeat on a `Returned` loan<br>3. Attempt cancel on a `Reviewed`/`Approved`/`Released` loan | 204 at `Submitted` and `Returned`; 400 at `Reviewed`, `Approved`, `Released`. `Cancelled` is terminal. | | |
| UT-039 | Path E — Multi-installment & partial payments | Released loan with generated payment schedules | 1. Applicant submit `5000` on schedule 1; Admin post `5000`<br>2. Applicant submit `5500` on schedule 1; Admin post `5500`<br>3. Repeat until every schedule is fully paid | Schedule flips `Pending → PartiallyPaid → Paid`; a schedule accepts multiple partial posts; loan reaches `Completed` **only** after every schedule is `Paid` (paying 11 of 12 must not complete it). | | |
| UT-040 | Path F — Insufficient treasury (rollback) | Approved loan whose amount exceeds the tenant treasury balance | 1. Admin `POST /{id}/release`<br>2. Admin `POST /api/treasury/deposit` to add funds<br>3. Admin `POST /{id}/release` again | Step 1 → 400 "Insufficient treasury funds..."; loan **stays `Approved`**; no `PaymentSchedule` rows created; treasury balance unchanged (fully rolled back). After deposit, retry → 204 (succeeds). | | |
| UT-041 | Path G — Identity-based routing (403 vs 404) | Tenant 1 token; a Tenant 2 loan id and a same-tenant other applicant's loan id | 1. Tenant 1 token `GET /{t2_loan_id}` (cross-tenant)<br>2. Applicant A `GET` Applicant B's loan (same tenant) | Cross-tenant → 404 (row filtered out — looks non-existent); same-tenant other user → 403 (row exists, access denied). Assert both — the split is a deliberate information-disclosure boundary. | | |

### Alternate-flow coverage checklist

- [ ] UT-035 (Path A) — Review→Return→resubmit→re-review loop (×2 iterations)
- [ ] UT-036 (Path B) — Approve→Return bounces to start; re-review required
- [ ] UT-037 (Path C) — Reject from review; Reject from approve; both terminal
- [ ] UT-038 (Path D) — Cancel valid at Submitted/Returned, blocked later
- [ ] UT-039 (Path E) — Partial payments; multi-post; completion only when all Paid
- [ ] UT-040 (Path F) — Insufficient-funds release rolls back cleanly, then succeeds after deposit
- [ ] UT-041 (Path G) — Cross-tenant 404 vs same-tenant-other-user 403
