<script setup lang="ts">
import { ref } from 'vue'

import { useTheme } from '~/composables'

const isDropdownOpen = ref(false)

const { themes, activeTheme, isLight, isDark, changeTheme } = useTheme()

function closeDropdown() {
  isDropdownOpen.value = false
}
</script>

<template>
  <nav class="relative">
    <ul class="flex items-center">
      <li>
        <RouterLink
          class="mr-6 text-sm font-medium hover:text-link"
          active-class="text-link"
          :to="{ name: 'codec' }"
        >
          Codec
        </RouterLink>
      </li>
      <li>
        <RouterLink
          class="text-sm font-medium hover:text-link"
          exact-active-class="text-link"
          :to="{ name: 'about' }"
        >
          About
        </RouterLink>
      </li>
      <li class="mx-6 border-l py-3"></li>
      <li @click="isDropdownOpen = true">
        <div
          class="cursor-pointer text-link"
          :class="{ 'i-heroicons-sun': isLight, 'i-heroicons-moon': isDark }"
        ></div>
        <ul
          v-if="isDropdownOpen"
          v-on-click-outside="closeDropdown"
          class="absolute top-16 right-0 w-36 rounded-lg border bg-white py-1 text-sm shadow-lg dark:bg-gray-800"
        >
          <li
            v-for="{ theme, text, icon } in themes"
            :key="theme"
            class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
            :class="{ 'text-link': theme === activeTheme }"
            @click="changeTheme(theme)"
          >
            <span
              :class="[
                'mr-2 text-gray-400',
                icon,
                { '!text-link': theme === activeTheme }
              ]"
            ></span>
            {{ text }}
          </li>
        </ul>
      </li>
      <li class="pl-4">
        <a
          href="https://github.com/JensDll/image-data-hiding"
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
