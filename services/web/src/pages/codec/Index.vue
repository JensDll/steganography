<script setup lang="ts"></script>

<template>
  <section>
    <nav
      class="mb-8 flex flex-col container sm:mb-12 sm:flex-row sm:items-center sm:justify-between"
    >
      <ul class="mb-12 flex space-x-4 sm:order-2 sm:m-0">
        <RouterLink
          v-slot="{ navigate, isExactActive }"
          :to="{ name: 'encode' }"
          custom
        >
          <li
            :class="[
              'cursor-pointer rounded-md bg-gray-100 py-2 px-6 font-medium hover:bg-gray-200',
              isExactActive && '!bg-encode-100 font-semibold text-encode-600'
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
              'cursor-pointer rounded-md bg-gray-100 py-2 px-6 font-medium hover:bg-gray-200',
              isExactActive && '!bg-decode-100 font-semibold text-decode-600'
            ]"
            @click="navigate"
          >
            Decode
          </li>
        </RouterLink>
      </ul>
      <h1 class="sm:order-1">{{ $route.meta.title }}</h1>
    </nav>
    <RouterView></RouterView>
  </section>
</template>

<style scoped></style>
