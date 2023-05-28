import { createReadStream, createWriteStream } from 'node:fs'
import fs from 'node:fs/promises'
import path from 'node:path'
import zlib from 'node:zlib'

import type { Plugin } from 'vite'

export interface Options {
  test?: RegExp
  limit?: number
}

export function Compression({
  test = /\.(js|css|html|txt|xml|json|svg|ico|ttf|otf|eot)$/,
  limit = 512
}: Options = {}): Plugin {
  return {
    name: 'compression',
    apply: 'build',
    async writeBundle({ dir }) {
      if (!dir) {
        throw new Error('[vite-plugin-compression] Missing output directory')
      }

      const promises: Promise<void>[] = []

      for await (const path of recursiveReadDir(dir, limit)) {
        if (!test.test(path)) {
          continue
        }

        promises.push(
          brotliCompressFile(path, {
            params: {
              [zlib.constants.BROTLI_PARAM_QUALITY]:
                zlib.constants.BROTLI_MAX_QUALITY
            }
          }),
          gzipCompressFile(path, {
            level: zlib.constants.Z_BEST_COMPRESSION
          })
        )
      }

      await Promise.all(promises)
    }
  }
}

async function* recursiveReadDir(
  dir: string,
  limit: number
): AsyncGenerator<string> {
  const dirents = await fs.readdir(dir, { withFileTypes: true })

  for (const dirent of dirents) {
    const resolvedPath = path.resolve(dir, dirent.name)

    if (dirent.isDirectory()) {
      yield* recursiveReadDir(resolvedPath, limit)
    } else {
      const stat = await fs.stat(resolvedPath)
      if (stat.size > limit) {
        yield resolvedPath
      }
    }
  }
}

function brotliCompressFile(path: string, options: zlib.BrotliOptions) {
  return new Promise<void>(resolve => {
    createReadStream(path)
      .pipe(zlib.createBrotliCompress(options))
      .pipe(createWriteStream(path + '.br'))
      .on('close', resolve)
  })
}

function gzipCompressFile(path: string, options: zlib.ZlibOptions) {
  return new Promise<void>(resolve => {
    createReadStream(path)
      .pipe(zlib.createGzip(options))
      .pipe(createWriteStream(path + '.gz'))
      .on('close', resolve)
  })
}
