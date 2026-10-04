import React, { forwardRef, useState } from 'react';
import styles from './Input.module.css';

export interface InputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size' | 'prefix'> {
  label?: string;
  error?: string;
  helperText?: string;
  size?: 'sm' | 'md' | 'lg';
  prefix?: React.ReactNode;
  suffix?: React.ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      error,
      helperText,
      size = 'md',
      prefix,
      suffix,
      className,
      disabled,
      onFocus,
      onBlur,
      id,
      ...props
    },
    ref
  ) => {
    const [isFocused, setIsFocused] = useState(false);
    const inputId = id || (label ? label.toLowerCase().replace(/\s+/g, '-') : undefined);

    const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
      setIsFocused(true);
      onFocus?.(e);
    };

    const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
      setIsFocused(false);
      onBlur?.(e);
    };

    return (
      <div className={`${styles.inputWrapper} ${className || ''}`}>
        {label && (
          <label htmlFor={inputId} className="text-secondary" style={{ fontSize: '13px', fontWeight: 500 }}>
            {label}
          </label>
        )}
        <div
          className={`
            ${styles.inputContainer}
            ${styles[`size-${size}`]}
            ${isFocused ? styles.isFocused : ''}
            ${error ? styles.hasError : ''}
            ${disabled ? styles.isDisabled : ''}
          `}
        >
          {prefix && <div className={styles.iconPrefix}>{prefix}</div>}
          <input
            ref={ref}
            id={inputId}
            disabled={disabled}
            onFocus={handleFocus}
            onBlur={handleBlur}
            className={styles.input}
            {...props}
          />
          {suffix && <div className={styles.iconSuffix}>{suffix}</div>}
        </div>
        {error && <span className={styles.errorText}>{error}</span>}
        {!error && helperText && <span className={styles.helperText}>{helperText}</span>}
      </div>
    );
  }
);

Input.displayName = 'Input';
