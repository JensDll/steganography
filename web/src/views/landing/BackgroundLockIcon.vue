<script setup lang="ts"></script>

<template>
  <div class="absolute inset-0 top-[10vh] grid justify-items-center">
    <svg
      xmlns="http://www.w3.org/2000/svg"
      class="w-[80vw] max-w-[900px] -rotate-6 opacity-20"
      viewBox="0 0 20 20"
    >
      <defs>
        <pattern
          id="peppers"
          width="1"
          height="1"
          patternContentUnits="objectBoundingBox"
        >
          <image href="/peppers.png" width="1" height="1" class="grayscale" />
        </pattern>
      </defs>
      <path
        fill-rule="evenodd"
        d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
        clip-rule="evenodd"
        fill="url(#peppers)"
      ></path>
    </svg>
  </div>
</template>

<style lang="postcss" scoped></style>
