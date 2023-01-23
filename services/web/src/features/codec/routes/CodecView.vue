<script setup lang="ts"></script>

<template>
  <section class="main">
    <header class="grid-area-[header]">
      <h1>
        {{ $route.meta.title }}
      </h1>
    </header>
    <nav class="nav grid-area-[nav]">
      <RouterLink
        :to="{ name: 'encode' }"
        class="link"
        exact-active-class="link-active"
      >
        Encode
      </RouterLink>
      <RouterLink
        :to="{ name: 'decode' }"
        class="link"
        exact-active-class="link-active"
      >
        Decode
      </RouterLink>
    </nav>
    <section class="grid-area-[main]">
      <RouterView></RouterView>
    </section>
  </section>
</template>

<style scoped>
.main {
  @apply container grid;
  grid-template-areas:
    'header'
    'nav'
    'main';
  grid-template-rows: auto 1fr;
}

.nav {
  @apply mb-12 grid border-b;
  grid-template-columns: 1fr 1fr;
}

.link {
  @apply py-2 text-center;
}

.link-active {
  @apply -mb-px border-b-2 !border-highlight bg-gray-100/80 font-medium text-highlight dark:bg-gray-800/50;
}

@screen md {
  .main {
    grid-template-rows: unset;
    @apply gap-y-0 gap-x-12;
    grid-template-columns: 10rem 1fr;
    grid-template-areas:
      'nav header'
      'nav main';
  }

  .nav {
    grid-template-columns: unset;
    border-bottom: unset;
    @apply m-0 gap-y-2 self-start;
    align-items: start;
  }

  .link {
    text-align: unset;
    border-bottom: unset;
    margin-bottom: unset;
    @apply border-l py-1 pl-6;
  }

  .link:hover {
    @apply border-gray-300 dark:border-gray-600;
  }
}

@screen lg {
  .main {
    @apply gap-x-16;
    grid-template-columns: 12rem 1fr;
  }
}
</style>
