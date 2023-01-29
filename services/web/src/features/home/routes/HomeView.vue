<script setup lang="ts"></script>

<template>
  <section class="grid h-full min-h-[600px] items-center">
    <div>
      <h1
        class="container mb-0 text-center text-4xl font-extrabold tracking-tight md:text-5xl 2xl:text-6xl"
      >
        Image Data Hiding
      </h1>
      <p class="container my-8 max-w-3xl text-center text-lg">
        Hide encrypted text and binary data in cover images.
      </p>
      <div class="mx-auto h-20 w-full max-w-2xl center-children">
        <BaseButton variant="landing" @click="$router.push({ name: 'encode' })">
          Getting Started
        </BaseButton>
      </div>
    </div>
  </section>
</template>
