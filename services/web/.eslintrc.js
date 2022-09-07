module.exports = {
  env: {
    browser: true,
    es2021: true,
    node: true,
    'vue/setup-compiler-macros': true
  },
  parser: 'vue-eslint-parser',
  parserOptions: {
    parser: '@typescript-eslint/parser',
    sourceType: 'module'
  },
  plugins: ['@typescript-eslint'],
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:vue/vue3-recommended',
    'prettier'
  ],
  ignorePatterns: ['dist', 'public', 'nginx'],
  rules: {
    // https://eslint.org/docs/latest/rules/sort-imports
    'sort-imports': [
      'error',
      {
        ignoreDeclarationSort: true
      }
    ],
    // https://eslint.org/docs/latest/rules/no-empty
    'no-empty': ['error', { allowEmptyCatch: true }],
    // https://typescript-eslint.io/rules/no-explicit-any
    '@typescript-eslint/no-explicit-any': 'off',
    // https://typescript-eslint.io/rules/no-var-requires
    '@typescript-eslint/no-var-requires': 'off',
    // https://typescript-eslint.io/rules/no-non-null-assertion
    '@typescript-eslint/no-non-null-assertion': 'off',
    // https://eslint.vuejs.org/rules/multi-word-component-names
    'vue/multi-word-component-names': [
      'error',
      {
        ignores: ['Index']
      }
    ]
  }
}
