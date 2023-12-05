<template>
  <header class="class flex items-center py-10 container">
    <Logo class="mr-12 h-7" />
    <nav class="flex">
      <NuxtLink
        to="/encode"
        class="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-100 hover:text-gray-900"
      >
        Encode
      </NuxtLink>
      <NuxtLink
        to="/decode"
        class="rounded-lg px-3 py-2 text-gray-700 hover:bg-gray-100 hover:text-gray-900"
      >
        Decode
      </NuxtLink>
    </nav>
  </header>

  <main class="pb-16 pt-8">
    <NuxtPage />
  </main>
</template>
