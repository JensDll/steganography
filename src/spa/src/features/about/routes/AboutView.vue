<script setup lang="ts"></script>

<template>
  <BaseMarkdown>
    <h1>About</h1>
    <p>
      Here you will find some notes about the project and how it works in a FAQ
      format.
    </p>
    <h2>How is the message hidden in the image?</h2>
    <p>
      The current algorithm implements a Least Significant Bit Substitution
      technique to hide the message. First, a pseudo-random number generator
      (PRNG) chooses a random distribution of pixel positions. Then, the message
      is processed bit for bit and placed at the least significant bit of the
      pixel value until nothing is left to embed.
    </p>
    <h2>What method is used for encryption?</h2>
    <p>
      Before writing the message to the image, the AES cipher encrypts it in
      Counter (CTR) mode. The implementation can be found in the
      <a
        href="https://github.com/JensDll/steganography/blob/staging/services/steganography.api/src/steganography.domain/AesCounterMode.cs"
      >
        source code
      </a>
      of the Web API project.
    </p>
    <h2>What information is part of the generated Base64 key?</h2>
    <ul>
      <li>
        The first 2-byte indicate the type of hidden message (text or binary)
      </li>
      <li>The next 4-byte store the seed for the PRNG</li>
      <li>Then another 4-byte for the message length</li>
      <li>
        The remaining 44-byte are used for the AES key (32-byte) and
        initialization value (12-byte)
      </li>
    </ul>
    <p>
      You have to make sure that only trusted parties know the key.
      Otherwise,&thinsp; you risk losing your data.
    </p>
  </BaseMarkdown>
</template>

<style scoped></style>
