<script setup lang="ts">
/* global changeTheme */
import { computed, ref, watch } from 'vue'

import type { OutlineIconName } from '~/components/app/AppIcon.vue'

type ThemeName = 'light' | 'dark' | 'system'
type Theme = {
  name: ThemeName
  text: string
  icon: OutlineIconName
}

const themes: Theme[] = [
  {
    name: 'light',
    text: 'Light',
    icon: 'Sun'
  },
  {
    name: 'dark',
    text: 'Dark',
    icon: 'Moon'
  },
  {
    name: 'system',
    text: 'System',
    icon: 'DesktopComputer'
  }
]

const isDropdownOpen = ref(false)
const isPopupOpen = ref(false)
const theme = ref<ThemeName>(localStorage.theme)

const isDark = ref(document.documentElement.classList.contains('dark'))
const isLight = computed(() => !isDark.value)

function changeThemePreference(themePreference: ThemeName) {
  changeTheme(themePreference)
  theme.value = themePreference
  isDark.value = document.documentElement.classList.contains('dark')
}

function closeDropdown() {
  isDropdownOpen.value = false
}

function closePopup() {
  isPopupOpen.value = false
}

watch(theme, changeThemePreference)
</script>

<template>
  <header
    class="sticky top-0 z-10 border-b bg-c-bg grid-area-[header]"
    :class="{ 'border-b-0': $route.name === 'home' }"
  >
    <div class="flex items-end justify-between py-6 container">
      <div
        class="cursor-pointer text-xl font-semibold"
        @click="$router.push({ name: 'home' })"
      >
        Steganography
      </div>
      <nav class="relative hidden md:block">
        <ul class="flex">
          <li class="font-medium hover:text-orange-500">
            <RouterLink :to="{ name: 'about' }">About</RouterLink>
          </li>
          <li class="mx-6 border-l"></li>
          <li @click="isDropdownOpen = true">
            <AppIcon
              v-if="isLight"
              class="h-6 w-6 cursor-pointer text-orange-600"
              outline="Sun"
            />
            <AppIcon
              v-if="isDark"
              class="h-6 w-6 cursor-pointer text-orange-600"
              outline="Moon"
            />
            <ul
              v-if="isDropdownOpen"
              v-on-click-outside="closeDropdown"
              class="absolute top-16 right-0 w-36 rounded-lg border bg-white py-1 text-sm shadow-lg dark:bg-gray-800"
            >
              <li
                v-for="{ name, text, icon } in themes"
                :key="name"
                class="flex cursor-pointer items-center py-1 px-2 font-semibold hover:bg-gray-50 dark:hover:bg-gray-600/30"
                :class="{ 'text-orange-600': theme === name }"
                @click="changeThemePreference(name)"
              >
                <AppIcon
                  class="mr-2 h-6 w-6 text-gray-400"
                  :class="{ '!text-orange-600': theme === name }"
                  :outline="icon"
                />
                {{ text }}
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
      <HeroiconsOutline:dotsVertical
        class="h-6 w-6 cursor-pointer hover:text-gray-500 md:hidden"
        @click="isPopupOpen = true"
      />
      <nav
        v-if="isPopupOpen"
        class="fixed inset-0 bg-gray-900/20 backdrop-blur-sm dark:bg-gray-900/50 md:hidden"
      >
        <div
          v-on-click-outside="closePopup"
          class="test fixed top-6 right-6 w-full max-w-xs rounded-lg bg-c-bg p-6 shadow-lg dark:bg-gray-800"
        >
          <ul class="space-y-6">
            <li>
              <RouterLink
                class="font-medium hover:text-orange-500"
                :to="{ name: 'about' }"
              >
                About
              </RouterLink>
            </li>
            <li>
              <a
                class="font-medium hover:text-orange-500"
                href="https://github.com/JensDll/image-data-hiding"
              >
                GitHub
              </a>
            </li>
          </ul>
          <div
            class="mt-5 flex items-center justify-between border-t border-c-form-border pt-5"
          >
            <label class="m-0" for="theme">Switch theme</label>
            <select
              id="theme"
              v-model="theme"
              class="dark:border-gray-600 dark:bg-gray-700"
            >
              <option
                v-for="{ name, text } in themes"
                :key="name"
                :value="name"
              >
                {{ text }}
              </option>
            </select>
          </div>
        </div>
      </nav>
    </div>
  </header>
</template>
