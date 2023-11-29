<script setup lang="ts"></script>

<template>
  <nav class="container">
    <ul class="reset-prose flex space-x-2">
      <li>
        <RouterLink :to="{ name: 'encode' }">Encode</RouterLink>
      </li>
      <li>
        <RouterLink :to="{ name: 'decode' }">Decode</RouterLink>
      </li>
    </ul>
  </nav>
  <RouterView />
</template>

<style scoped></style>
