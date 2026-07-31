# Azure deployment guide

This project uses SQL Server through EF Core, so the simplest deployment is:

```text
Azure App Service (Free F1) → Azure SQL Database (Free offer)
```

The free App Service tier is intended for testing and has a daily CPU quota. The Azure SQL free offer has monthly compute and storage limits. Monitor both services in Azure Cost Management.

## 1. Create Azure SQL Database

Create an Azure SQL Database and select the **Free offer** during provisioning. Choose the option to auto-pause when the free allowance is exhausted so the database does not continue into billable usage.

Record the SQL connection string. It will be configured in App Service, not committed to this repository.

## 2. Create the App Service

Create a .NET 8 App Service and choose the **Free F1** plan. The default URL will look like:

```text
https://your-app-name.azurewebsites.net
```

Configure these App Service application settings:

```text
ASPNETCORE_ENVIRONMENT=Production
Authentication__SecretForKey=<the same JWT secret used by the API>
Authentication__Issuer=loanappapi
Authentication__Audience=loanappclient
```

Add the database under **Connection strings** with the name `DefaultConnection`. ASP.NET Core exposes it to the application as `ConnectionStrings__DefaultConnection`.

Allow Azure services through the SQL server firewall so App Service can connect.

## 3. Configure GitHub deployment

The repository includes `.github/workflows/deploy-azure-api.yml`. Add these GitHub repository secrets:

```text
AZURE_WEBAPP_NAME
AZURE_WEBAPP_PUBLISH_PROFILE
```

Download the App Service publish profile from Azure and store its full contents as `AZURE_WEBAPP_PUBLISH_PROFILE`. Run the workflow manually from the Actions tab.

The workflow builds and deploys the API and uploads an idempotent SQL migration script as the `azure-migrations` artifact.

## 4. Apply the database migration

Download `azure-migrations.sql` from the workflow artifact and run it against the Azure SQL Database using the Azure portal Query Editor or SQL Server Management Studio.

The API intentionally applies migrations automatically only in Development. Production schema changes should be applied before or during deployment through this migration script.

## 5. Give QA the Postman environment

Import both:

- `postman/LoanApplicationPlatform.postman_collection.json`
- `postman/LoanApplicationPlatform.postman_environment.azure.json`

Select **Loan Application Platform - Azure** and replace `baseUrl` with the deployed App Service URL. Then run the login requests; the collection scripts will cache the JWTs automatically.

The seeded accounts are documented in `QA_TESTING_GUIDE.md`. QA does not need this repository, .NET, SQL Server, or any local dependencies.
