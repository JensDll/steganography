<script setup lang="ts"></script>

<template>
  <header class="z-10 border-b bg-slate-50 grid-area-[header]">
    <div
      class="flex items-end justify-between px-4 py-6 md:container md:mx-auto lg:px-6"
    >
      <div class="text-xl font-semibold">Steganography</div>
      <nav>
        <ul class="flex">
          <li class="mr-4 font-medium hover:text-sky-400">
            <router-link to="#">About</router-link>
          </li>
          <li class="border-l pl-4">
            <a
              href="https://github.com/JensDll/image-data-hiding"
              class="text-slate-400 hover:text-slate-500"
            >
              <mdi:github class="h-6 w-6" />
            </a>
          </li>
        </ul>
      </nav>
    </div>
  </header>
</template>

<style lang="postcss" scoped></style>
