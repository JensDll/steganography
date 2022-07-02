<script setup lang="ts"></script>

<template>
  <section>
    <nav
      class="mb-8 flex flex-col container sm:mb-16 sm:flex-row sm:items-end sm:justify-between"
    >
      <h2 class="mb-6 sm:order-1 sm:mb-0">{{ $route.meta.title }}</h2>
      <ul
        class="mb-4 flex space-x-4 border-b sm:order-2 sm:mb-0 sm:translate-y-2"
      >
        <RouterLink
          v-slot="{ navigate, isExactActive }"
          :to="{ name: 'encode' }"
          custom
        >
          <li
            :class="[
              'cursor-pointer py-3 px-4 font-semibold hover:-mb-px hover:border-b-2 hover:border-gray-300 dark:hover:border-gray-700',
              isExactActive &&
                '-mb-px border-b-2 !border-encode-500 text-encode-500 dark:border-encode-600'
            ]"
            @click="navigate"
          >
            Encode
          </li>
        </RouterLink>
        <RouterLink
          v-slot="{ navigate, isExactActive }"
          :to="{ name: 'decode' }"
          custom
        >
          <li
            :class="[
              'cursor-pointer py-3 px-4 font-semibold hover:-mb-px hover:border-b-2 hover:border-gray-300 dark:hover:border-gray-700',
              isExactActive &&
                '-mb-px border-b-2 !border-decode-500 text-decode-500 dark:border-decode-600'
            ]"
            @click="navigate"
          >
            Decode
          </li>
        </RouterLink>
      </ul>
    </nav>
    <RouterView></RouterView>
  </section>
</template>

<style scoped></style>
