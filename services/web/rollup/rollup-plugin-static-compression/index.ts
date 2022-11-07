import { createReadStream, createWriteStream } from 'node:fs'
import fs from 'node:fs/promises'
import path from 'node:path'
import zlip from 'node:zlib'

import type { Plugin } from 'vite'

export interface Options {
  test?: RegExp
}

export function Compression({
  test = /\.(js|css|html|txt|xml|json|svg|ico|ttf|otf|eot)$/
}: Options = {}): Plugin {
  return {
    name: 'compression',
    apply: 'build',

    async writeBundle({ dir }) {
      if (!dir) {
        return
      }

      const promises: Promise<void>[] = []

      for await (const path of recursiveReadDir(dir)) {
        if (test.test(path)) {
          promises.push(
            brotliCompressFile(path, {
              params: {
                [zlip.constants.BROTLI_PARAM_QUALITY]: 11
              }
            }),
            gzipCompressFile(path, {
              level: zlip.constants.Z_BEST_COMPRESSION
            })
          )
        }
      }

      await Promise.all(promises)
    }
  }
}

async function* recursiveReadDir(dir: string): AsyncGenerator<string> {
  const dirents = await fs.readdir(dir, { withFileTypes: true })

  for (const dirent of dirents) {
    const resolvedPath = path.resolve(dir, dirent.name)

    if (dirent.isDirectory()) {
      yield* recursiveReadDir(resolvedPath)
    } else {
      yield resolvedPath
    }
  }
}

function brotliCompressFile(path: string, options: zlip.BrotliOptions) {
  return new Promise<void>(resolve => {
    createReadStream(path)
      .pipe(zlip.createBrotliCompress(options))
      .pipe(createWriteStream(path + '.br'))
      .on('close', resolve)
  })
}

function gzipCompressFile(path: string, options: zlip.ZlibOptions) {
  return new Promise<void>(resolve => {
    createReadStream(path)
      .pipe(zlip.createGzip(options))
      .pipe(createWriteStream(path + '.gz'))
      .on('close', resolve)
  })
}
