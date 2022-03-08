<script setup lang="ts"></script>

<template>
  <header
    class="z-10 border-b bg-gray-50 grid-area-[header]"
    :class="{ 'border-b-0': $route.name === 'home' }"
  >
    <div class="flex items-end justify-between py-6 container">
      <div
        class="cursor-pointer text-xl font-semibold"
        @click="$router.push({ name: 'home' })"
      >
        Steganography
      </div>
      <nav>
        <ul class="flex">
          <li class="mr-4 font-medium hover:text-blue-400">
            <RouterLink to="#">About</RouterLink>
          </li>
          <li class="border-l pl-4">
            <a
              href="https://github.com/JensDll/image-data-hiding"
              class="text-gray-400 hover:text-gray-500"
            >
              <Mdi:github class="h-6 w-6" />
            </a>
          </li>
        </ul>
      </nav>
    </div>
  </header>
</template>
