<script setup lang="ts"></script>

<template>
  <the-header />
  <the-main />
</template>

<style lang="postcss">
@tailwind base;
@tailwind components;
@tailwind utilities;

#app {
  height: 100vh;
  display: grid;
  grid-template-columns: 1fr;
  grid-template-rows: auto 1fr;
  grid-template-areas:
    'header'
    'main';
}

@layer base {
  body {
    font-family: 'Montserrat Alternates', sans-serif;
  }

  h1 {
    @apply text-4xl font-extrabold text-slate-900 sm:text-5xl lg:text-6xl;
  }

  h2 {
    @apply text-3xl font-bold text-slate-900 sm:text-4xl lg:text-5xl;
  }

  h3 {
    @apply text-2xl font-semibold text-slate-900 sm:text-3xl lg:text-4xl;
  }

  h4 {
    @apply text-xl font-semibold text-slate-900 sm:text-2xl lg:text-3xl;
  }
}

@layer components {
  .label {
    @apply mb-1 block font-medium text-slate-700;
  }

  .input {
    @apply block rounded-md border border-slate-300 bg-white p-2;
  }
  .input:focus,
  .input:focus-within {
    @apply border-indigo-300 ring ring-indigo-200 ring-opacity-50;
  }
  .input.error {
    @apply border-red-400;
  }
  .input.error:focus,
  .input.error:focus-within {
    @apply border-red-300 ring ring-red-200 ring-opacity-50;
  }
}
</style>
