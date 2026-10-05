import { execFileSync } from 'node:child_process'
import { mkdtemp, readFile, rm } from 'node:fs/promises'
import { tmpdir } from 'node:os'
import { join } from 'node:path'
import { fileURLToPath } from 'node:url'

const versionedPath = fileURLToPath(new URL('../src/features/tasks/api/schema.ts', import.meta.url))
const temporaryDirectory = await mkdtemp(join(tmpdir(), 'todolist-api-types-'))
const generatedPath = join(temporaryDirectory, 'schema.ts')

try {
  execFileSync(process.execPath, [fileURLToPath(new URL('./generate-api-types.mjs', import.meta.url)), generatedPath])
  const [versioned, generated] = await Promise.all([
    readFile(versionedPath, 'utf8'),
    readFile(generatedPath, 'utf8'),
  ])
  if (versioned !== generated) {
    console.error('Os tipos TypeScript estão divergentes. Execute npm run generate:api --prefix frontend.')
    process.exitCode = 1
  } else {
    console.log('Tipos TypeScript sincronizados com o contrato aprovado.')
  }
} finally {
  await rm(temporaryDirectory, { recursive: true, force: true })
}
