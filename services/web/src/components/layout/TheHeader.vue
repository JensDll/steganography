<script setup lang="ts">
/* global changeTheme */
import { computed, ref } from 'vue'

type Theme = 'light' | 'dark' | 'system'

const isDropdownOpen = ref(false)
const theme = ref<Theme>(localStorage.theme)

const isDark = ref(document.documentElement.classList.contains('dark'))
const isLight = computed(() => !isDark.value)

function changeThemePreference(themePreference: Theme) {
  localStorage.setItem('theme', themePreference)
  theme.value = themePreference
  changeTheme()
  isDark.value = document.documentElement.classList.contains('dark')
}

function closeDropdown() {
  isDropdownOpen.value = false
}
</script>

<template>
  <header
    class="z-10 border-b grid-area-[header]"
    :class="{ 'border-b-0': $route.name === 'home' }"
  >
    <div class="flex items-end justify-between py-6 container">
      <div
        class="cursor-pointer text-xl font-semibold"
        @click="$router.push({ name: 'home' })"
      >
        Steganography
      </div>
      <nav class="relative">
        <ul class="flex">
          <li class="font-medium hover:text-orange-500">
            <RouterLink to="#">About</RouterLink>
          </li>
          <li class="mx-6 border-l"></li>
          <li @click="isDropdownOpen = true">
            <HeroiconsOutline:sun
              v-if="isLight"
              class="h-6 w-6 cursor-pointer text-orange-600"
            />
            <HeroiconsOutline:moon
              v-if="isDark"
              class="h-6 w-6 cursor-pointer text-orange-600"
            />
            <ul
              v-if="isDropdownOpen"
              v-on-click-outside="closeDropdown"
              class="absolute top-16 right-0 w-36 rounded-lg border bg-white py-1 text-sm shadow-lg dark:bg-gray-800"
            >
              <li
                class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
                :class="{ 'text-orange-600': theme === 'light' }"
                @click="changeThemePreference('light')"
              >
                <HeroiconsOutline:sun
                  class="mr-2 h-6 w-6 text-gray-400"
                  :class="{ '!text-orange-600': theme === 'light' }"
                />
                Light
              </li>
              <li
                class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
                :class="{ 'text-orange-600': theme === 'dark' }"
                @click="changeThemePreference('dark')"
              >
                <HeroiconsOutline:moon
                  class="mr-2 h-6 w-6 text-gray-400"
                  :class="{ '!text-orange-600': theme === 'dark' }"
                />
                Dark
              </li>
              <li
                class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
                :class="{ 'text-orange-500': theme === 'system' }"
                @click="changeThemePreference('system')"
              >
                <HeroiconsOutline:desktopComputer
                  class="mr-2 h-6 w-6 text-gray-400"
                  :class="{ '!text-orange-600': theme === 'system' }"
                />
                System
              </li>
            </ul>
          </li>
          <li class="pl-4">
            <a href="https://github.com/JensDll/image-data-hiding">
              <Mdi:github
                class="h-6 w-6 text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
              />
            </a>
          </li>
        </ul>
      </nav>
    </div>
  </header>
</template>
