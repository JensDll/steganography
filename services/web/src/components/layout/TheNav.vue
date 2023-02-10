<script setup lang="ts">
import { ref } from 'vue'

import { useTheme } from '~/composables'

const isDropdownOpen = ref(false)

const { themes, activeTheme } = useTheme()

function closeDropdown() {
  isDropdownOpen.value = false
}
</script>

<template>
  <nav class="relative text-sm">
    <ul class="flex items-center">
      <li class="mr-6">
        <RouterLink
          class="font-medium hover:text-link"
          active-class="text-link"
          :to="{ name: 'codec' }"
        >
          Codec
        </RouterLink>
      </li>
      <li class="mr-12">
        <RouterLink
          class="font-medium hover:text-link"
          exact-active-class="text-link"
          :to="{ name: 'about' }"
        >
          About
        </RouterLink>
      </li>
      <li class="mr-4" @click="isDropdownOpen = true">
        <div
          class="cursor-pointer text-link"
          :class="activeTheme.activeIcon"
        ></div>
        <ul
          v-if="isDropdownOpen"
          v-on-click-outside="closeDropdown"
          class="absolute top-12 right-0 w-32 rounded-lg border border-border-form bg-white py-1 shadow dark:bg-gray-800"
        >
          <li
            v-for="theme in themes"
            :key="theme.name"
            class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
            :class="{ 'text-link': theme.isActive }"
            @click="activeTheme = theme"
          >
            <span
              class="mr-2 text-gray-400"
              :class="[theme.icon, { '!text-link': theme.isActive }]"
            ></span>
            {{ theme.text }}
          </li>
        </ul>
      </li>
      <li>
        <a
          href="https://github.com/JensDll/steganography"
          aria-label="Link to the source code on GitHub"
        >
          <div
            class="i-mdi-github text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
          ></div>
        </a>
      </li>
    </ul>
  </nav>
</template>

<style scoped></style>
