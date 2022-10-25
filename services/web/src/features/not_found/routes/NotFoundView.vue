<script setup lang="ts"></script>

<template>
  <BaseMarkdown class="max-w-container text-center">
    <h1>{{ $route.meta.title }}</h1>
    <p>Sorry, it looks like the requested resource does not exist.</p>
    <RouterLink :to="{ name: 'home' }" class="mx-auto flex w-fit items-center">
      <div class="i-heroicons-arrow-left-20-solid mr-1"></div>
      Go Home
    </RouterLink>
  </BaseMarkdown>
</template>

<style scoped></style>
