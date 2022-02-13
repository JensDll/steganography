<script setup lang="ts"></script>

<template>
  <section
    id="mainSection"
    class="mt-48 grid grid-rows-[auto_1fr] items-start justify-items-center px-4 lg:px-6"
  >
    <nav class="mb-16">
      <ul class="flex">
        <RouterLink
          v-slot="{ navigate, isActive, isExactActive }"
          :to="{ name: 'encode' }"
          custom
        >
          <li
            :class="[
              'cursor-pointer rounded-md py-2 px-6 text-xl font-medium hover:text-sky-500',
              isActive && 'router-link-active',
              isExactActive && 'bg-sky-50 font-semibold text-sky-600'
            ]"
            @click="navigate"
          >
            Encode
          </li>
        </RouterLink>
        <RouterLink
          v-slot="{ navigate, isActive, isExactActive }"
          :to="{ name: 'decode' }"
          custom
        >
          <li
            :class="[
              'cursor-pointer rounded-md py-2 px-6 text-xl font-medium hover:text-emerald-500',
              isActive && 'router-link-active',
              isExactActive && 'bg-emerald-50 font-semibold text-emerald-600'
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
