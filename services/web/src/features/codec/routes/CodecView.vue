<script setup lang="ts"></script>

<template>
  <section>
    <nav
      class="container mb-8 flex flex-col sm:mb-12 sm:flex-row sm:items-end sm:justify-between lg:mb-16"
    >
      <h1 class="mb-6 sm:order-1 sm:mb-0">
        {{ $route.meta.title }}
      </h1>
      <nav
        class="mb-4 flex space-x-4 border-b sm:order-2 sm:mb-0 sm:translate-y-2"
      >
        <RouterLink
          v-for="name in ['encode', 'decode']"
          :key="name"
          :to="{ name }"
          class="py-3 px-4 capitalize hover:-mb-px hover:border-b-2 hover:border-gray-300 dark:hover:border-gray-700"
          exact-active-class="-mb-px border-b-2 !border-highlight text-highlight"
        >
          {{ name }}
        </RouterLink>
      </nav>
    </nav>
    <RouterView></RouterView>
  </section>
</template>

<style scoped></style>
