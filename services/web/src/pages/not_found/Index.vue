<script setup lang="ts"></script>

<template>
  <section class="container">
    <h2 class="mb-5">Not Found</h2>
    <p>Sorry, it looks like the requested resource does not exist.</p>
    <AppButton class="mt-8" variant="encode" @click="$router.back()">
      Go back
    </AppButton>
  </section>
</template>

<style scoped></style>
