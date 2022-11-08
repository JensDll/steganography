<script setup lang="ts">
import { useTheme } from '~/composables'

defineEmits(['close'])

const { activeTheme, themes } = useTheme()
</script>

<template>
  <nav
    class="fixed inset-0 z-50 bg-gray-900/20 backdrop-blur-sm dark:bg-gray-900/50"
    @click.self="$emit('close')"
  >
    <div
      class="fixed top-5 right-5 left-5 rounded-lg bg-fill p-6 shadow-lg dark:bg-gray-800 sm:left-auto sm:w-full sm:max-w-xs"
    >
      <div
        class="i-heroicons-x-mark absolute top-6 right-6 cursor-pointer text-gray-500 hover:text-gray-600 dark:text-gray-400 dark:hover:text-gray-300"
        @click="$emit('close')"
      ></div>
      <ul class="space-y-6">
        <li>
          <RouterLink
            class="mr-6 font-medium hover:text-link"
            active-class="text-link"
            :to="{ name: 'codec' }"
          >
            Codec
          </RouterLink>
        </li>
        <li>
          <RouterLink
            class="font-medium hover:text-link"
            exact-active-class="text-link"
            :to="{ name: 'about' }"
          >
            About
          </RouterLink>
        </li>
        <li>
          <a
            class="cursor-pointer font-medium hover:text-link"
            href="https://github.com/JensDll/image-data-hiding"
          >
            GitHub
          </a>
        </li>
      </ul>
      <div
        class="mt-5 flex items-center justify-between border-t border-border-form pt-5"
      >
        <label class="m-0" for="theme">Switch theme</label>
        <select
          id="theme"
          v-model="activeTheme"
          class="dark:border-gray-600 dark:bg-gray-700"
        >
          <option v-for="{ theme, text } in themes" :key="theme" :value="theme">
            {{ text }}
          </option>
        </select>
      </div>
    </div>
  </nav>
</template>

<style scoped></style>
