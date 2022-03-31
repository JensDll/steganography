<script setup lang="ts"></script>

<template>
  <AppMarkdown>
    <div class="max-w-prose">
      <h1>About</h1>
      <p>Some notes about how this works and implementation details.</p>
      <h2>How is the message hidden in the image?</h2>
      <p>
        The current algorithm implements a Least Significant Bit Substitution
        technique to hide the message. First, a pseudo-random number generator
        (PRNG) chooses a random distribution of pixel positions. Then, the
        message is processed bit for bit and placed at the least significant bit
        of the pixel value until nothing is left to embed.
      </p>
      <h2>What method is used for encryption?</h2>
      <p>
        Before writing the message, the AES cipher encrypts it in Counter (CTR)
        mode. The implementation can be found in the
        <a
          href="https://github.com/JensDll/image-data-hiding/blob/main/services/api/src/Domain/Entities/AesCounterMode.cs"
          >source code</a
        >
        of the Web API project.
      </p>
      <h2>What information is part of the generated Base64 key?</h2>
      <ul>
        <li>
          The first two bytes indicate the type of hidden message (text or
          binary)
        </li>
        <li>The next four bytes store the seed for the PRNG</li>
        <li>Then another four bytes for the message length</li>
        <li>
          The remaining 44 bytes are used for the AES key (32 bytes) and
          initialization value (12 bytes)
        </li>
      </ul>
      That makes for a key length of 54 bytes or
      <code>(54 / 3) * 4 = 72</code> Base64 characters (<a
        href="https://github.com/JensDll/image-data-hiding/blob/main/services/api/src/Domain/Services/KeyService.cs"
        >source code</a
      >). You have to make sure that only trusted parties know the key;
      otherwise, you risk losing your data. A generated key can not be revoked.
      The best you can do is to re-encrypt the message, but this will only work
      if any adversaries do not also possess the cover image.
    </div>
  </AppMarkdown>
</template>

<style scoped></style>
