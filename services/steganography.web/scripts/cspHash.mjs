import { createHash } from 'node:crypto'

const result = createHash('sha256').update(process.argv[2]).digest('base64')
console.log(result)
