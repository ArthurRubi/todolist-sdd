import { writeFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import openapiTS, { astToString } from 'openapi-typescript'

const contractUrl = new URL('../../docs/specs/001-essential-task/contracts/tasks-api.openapi.yaml', import.meta.url)
const outputUrl = new URL('../src/features/tasks/api/schema.ts', import.meta.url)
const nodes = await openapiTS(contractUrl)
const banner = '// Gerado de docs/specs/001-essential-task/contracts/tasks-api.openapi.yaml. Não editar manualmente.\n'

await writeFile(fileURLToPath(outputUrl), `${banner}${astToString(nodes)}`, 'utf8')
