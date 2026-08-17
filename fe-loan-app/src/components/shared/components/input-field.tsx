import { ChangeEvent, useEffect, useRef, useState } from 'react'
import { FaEye, FaEyeSlash } from 'react-icons/fa'

import { cn } from '@/lib/utils'
import { Input } from '@/components/ui/input'
import DataMap from '@/utils/data-map'

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

type IInputFieldOption = {
  label: string
  value: string
}

type IInputField = {
  label?: string
  labelStyle?: string
  fieldType?: 'text' | 'email' | 'password' | 'number' | 'select' | string
  value?: string
  onChange?: (e: ChangeEvent<HTMLInputElement>) => void
  onValueChange?: (value: string) => void
  options?: IInputFieldOption[]
  inputStyle?: string
  isRequired?: boolean
  size?: 'sm' | 'md' | 'lg' | 'xl'
  readOnly?: boolean
  disabled?: boolean
  placeholder?: string
  min?: string
  max?: string
  errorMessage?: string
}

const SIZE: Record<NonNullable<IInputField['size']>, string> = {
  sm: 'h-7 py-0 text-[0.8rem]',
  md: 'h-8 py-0 text-[0.85rem]',
  lg: 'h-9 py-0',
  xl: 'h-11 py-0 text-base md:text-base',
}

const InputField = ({
  label,
  labelStyle,
  fieldType = 'text',
  value,
  onChange,
  onValueChange,
  options,
  inputStyle,
  isRequired,
  size,
  readOnly,
  disabled,
  placeholder,
  min,
  max,
  errorMessage,
}: IInputField) => {
  const [showPassword, setShowPassword] = useState(false)

  const [numberText, setNumberText] = useState(value ?? '')
  const numberFocused = useRef(false)

  useEffect(() => {
    if (numberFocused.current) return
    const current = numberText.trim() === '' ? '' : Number(numberText)
    const incoming = value === undefined || value.trim() === '' ? '' : Number(value)
    if (current !== incoming) setNumberText(value ?? '')
  }, [value, numberText])

  const handleNumberChange = (e: ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value
    if (raw !== '' && !/^\d*\.?\d*$/.test(raw)) return
    setNumberText(raw)
    onChange?.(e)
  }

  const sizeClass = SIZE[size ?? 'md']

  const isPassword = fieldType === 'password'
  const isNumber = fieldType === 'number'
  const isSelect = fieldType === 'select'

  return (
    <div className="flex w-full flex-col gap-1">
      {label && (
        <div className={`${labelStyle}`}>
          <span className="font-semibold text-lg">{label}</span>{' '}
          {isRequired && <span className="text-destructive">*</span>}
        </div>
      )}

      {isSelect && (
        <Select
          items={options ?? []}
          value={value}
          onValueChange={(next: string | null) => onValueChange?.(next ?? '')}
          disabled={disabled}
        >
          <SelectTrigger
            className={cn(sizeClass, 'w-full bg-white', inputStyle)}
            aria-invalid={!!errorMessage}
          >
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
          <SelectContent>
            <DataMap
              data={options ?? []}
              render={(option) => (
                <SelectItem key={option.value} value={option.value} label={option.label}>
                  {option.label}
                </SelectItem>
              )}
            />
          </SelectContent>
        </Select>
      )}

      {isPassword && (
        <div className="relative inline-block">
          <Input
            type={showPassword ? 'text' : 'password'}
            className={cn(sizeClass, 'pr-9', inputStyle)}
            onChange={onChange}
            value={value}
            required={isRequired}
            readOnly={readOnly ?? !onChange}
            disabled={disabled}
            placeholder={placeholder}
            aria-invalid={!!errorMessage}
          />
          <button
            type="button"
            className="absolute top-1/2 right-2 -translate-y-1/2 p-1 text-muted-foreground hover:text-foreground"
            onClick={() => setShowPassword((prev) => !prev)}
            aria-label={showPassword ? 'Hide password' : 'Show password'}
          >
            {showPassword ? <FaEye /> : <FaEyeSlash />}
          </button>
        </div>
      )}

      {isNumber && (
        <Input
          type="text"
          inputMode="decimal"
          className={cn(sizeClass, inputStyle)}
          onChange={handleNumberChange}
          onFocus={() => {
            numberFocused.current = true
          }}
          onBlur={() => {
            numberFocused.current = false
            setNumberText(value ?? '')
          }}
          value={numberText}
          required={isRequired}
          readOnly={readOnly ?? !onChange}
          disabled={disabled}
          min={0}
          placeholder={placeholder}
          aria-invalid={!!errorMessage}
        />
      )}

      {!isPassword && !isNumber && !isSelect && (
        <Input
          type={fieldType}
          className={cn(sizeClass, inputStyle)}
          onChange={onChange}
          value={value}
          required={isRequired}
          readOnly={readOnly ?? !onChange}
          disabled={disabled}
          placeholder={placeholder}
          min={min}
          max={max}
          aria-invalid={!!errorMessage}
        />
      )}

      {errorMessage && <p className="text-xs text-destructive">{errorMessage}</p>}
    </div>
  )
}

export default InputField
