import tseslint from 'typescript-eslint'
import reactHooks from 'eslint-plugin-react-hooks'

// Flat-config port of the project's legacy `.eslintrc.js` rule set.
// The legacy config intentionally enumerates every non-recommended rule
// explicitly and does NOT extend eslint:recommended / tseslint recommended,
// so this config only enables the rules listed below.
export default tseslint.config(
  {
    ignores: [
      'node_modules',
      'dist',
      'karma.conf.js',
      '**/polyfills.ts',
      '**/*.spec.ts',
      '**/commands.ts',
      '**/app.po.ts',
      '**/middleware.js',
      '**/*.mock.ts',
      '**/webpack.config.js',
    ],
  },
  {
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      parser: tseslint.parser,
      ecmaVersion: 'latest',
      sourceType: 'module',
      parserOptions: {
        ecmaFeatures: { tsx: true },
      },
    },
    plugins: {
      '@typescript-eslint': tseslint.plugin,
      'react-hooks': reactHooks,
    },
    rules: {
      // React Hooks
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'warn',

      // Possible Problems
      'array-callback-return': 'off',
      'no-await-in-loop': 'off',
      'no-class-assign': 'error',
      'no-constant-binary-expression': 'error',
      'no-constructor-return': 'error',
      'no-duplicate-imports': 'error',
      'no-new-native-nonconstructor': 'error',
      'no-promise-executor-return': 'error',
      'no-undef': ['off', { typeof: true }],
      'no-self-compare': 'error',
      'no-template-curly-in-string': 'error',
      'no-unmodified-loop-condition': 'warn',
      'no-unreachable-loop': 'error',
      'no-unused-private-class-members': 'error',
      'require-atomic-updates': 'off',

      // Suggestions
      'arrow-body-style': 'error',
      'block-scoped-var': 'error',
      'consistent-return': 'off',
      curly: ['error', 'multi-line'],
      'default-case': 'error',
      eqeqeq: ['error', 'smart'],
      'id-length': ['error', { min: 1 }],
      'new-cap': ['error', { newIsCap: true, capIsNew: false }],
      'no-bitwise': 'error',
      'no-caller': 'error',
      'no-console': 'error',
      'no-continue': 'error',
      'no-else-return': ['error', { allowElseIf: false }],
      'no-eq-null': 'error',
      'no-eval': 'error',
      'no-implied-eval': 'error',
      'no-lone-blocks': 'error',
      'no-loop-func': 'error',
      'no-new-object': 'error',
      'no-new-wrappers': 'error',
      'no-nested-ternary': 'off',
      'no-restricted-syntax': ['error', 'ForInStatement', 'LabeledStatement', 'WithStatement'],
      'no-return-assign': ['off', 'always'],
      'no-sequences': 'error',
      'no-throw-literal': 'off',
      'no-undef-init': 'error',
      'no-useless-return': 'error',
      'no-var': 'error',
      'one-var': ['error', 'never'],
      'prefer-arrow-callback': 'error',
      'prefer-const': 'error',
      'prefer-template': 'error',
      'quote-props': ['error', 'as-needed', { keywords: false, unnecessary: true, numbers: false }],
      radix: 'error',
      'spaced-comment': ['error', 'always', { markers: ['/'] }],
      yoda: 'error',

      // Disabled in favor of TypeScript equivalents / per legacy config
      // (these keys were overridden to 0 by later duplicate keys in the
      //  legacy rules object, so they resolve to 'off').
      'dot-notation': 'off',
      'no-empty-function': 'off',
      'no-shadow': 'off',
      'no-unused-expressions': 'off',
      'no-use-before-define': 'off',

      // TypeScript
      '@typescript-eslint/array-type': ['error', { default: 'array' }],
      '@typescript-eslint/consistent-type-assertions': 'error',
      '@typescript-eslint/consistent-type-definitions': 'off',
      '@typescript-eslint/dot-notation': 'off',
      '@typescript-eslint/explicit-member-accessibility': [
        'error',
        {
          accessibility: 'explicit',
          overrides: { parameterProperties: 'explicit' },
        },
      ],
      '@typescript-eslint/explicit-module-boundary-types': [
        'off',
        {
          allowTypedFunctionExpressions: true,
          allowHigherOrderFunctions: false,
          allowDirectConstAssertionInArrowFunctions: true,
          allowArgumentsExplicitlyTypedAsAny: false,
        },
      ],
      '@typescript-eslint/member-ordering': 'error',
      '@typescript-eslint/naming-convention': [
        'error',
        {
          selector: 'variable',
          format: ['camelCase', 'UPPER_CASE', 'PascalCase', 'snake_case'],
          leadingUnderscore: 'allow',
          trailingUnderscore: 'forbid',
        },
      ],
      '@typescript-eslint/no-empty-function': ['off', { allow: ['overrideMethods'] }],
      '@typescript-eslint/no-shadow': 'off',
      '@typescript-eslint/no-non-null-assertion': 'off',
      '@typescript-eslint/no-unused-expressions': 'off',
      '@typescript-eslint/no-use-before-define': 'off',

      '@typescript-eslint/no-explicit-any': 'warn',
      '@typescript-eslint/no-inferrable-types': 'off',
      '@typescript-eslint/typedef': [
        'error',
        {
          parameter: true,
          propertyDeclaration: true,
          objectDestructuring: false,
          arrayDestructuring: false,
        },
      ],
      '@typescript-eslint/unified-signatures': 'error',
    },
  },
)
