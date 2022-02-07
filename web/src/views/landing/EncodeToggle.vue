<script setup lang="ts"></script>

<template>
  <nav class="flex items-center">
    <router-link
      custom
      :to="{ name: 'encode' }"
      v-slot="{ isExactActive, navigate }"
    >
      <span
        :class="[
          'cursor-pointer',
          !isExactActive && 'opacity-20 hover:opacity-40'
        ]"
        @click="navigate"
      >
        Encode
      </span>
    </router-link>
    <app-toggle
      :class-names="{
        toggle: 'mx-4 border-sky-200 bg-sky-50',
        handle: 'bg-sky-500'
      }"
      :model-value="$route.name === 'decode'"
      :duration="300"
      @toggle="
        isActive => $router.replace({ name: isActive ? 'decode' : 'encode' })
      "
    >
      <template #active>
        <mdi:lock-open-variant class="text-white" />
      </template>
      <template #inactive>
        <mdi:lock class="text-white" />
      </template>
    </app-toggle>
    <router-link
      custom
      :to="{ name: 'decode' }"
      v-slot="{ isExactActive, navigate }"
    >
      <span
        :class="[
          'cursor-pointer',
          !isExactActive && 'opacity-20 hover:opacity-40'
        ]"
        @click="navigate"
      >
        Decode
      </span>
    </router-link>
  </nav>
</template>

<style lang="postcss" scoped></style>
