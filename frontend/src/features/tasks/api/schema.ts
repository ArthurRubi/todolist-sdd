// Gerado de docs/specs/001-essential-task/contracts/tasks-api.openapi.yaml. Não editar manualmente.
export interface paths {
    "/tasks": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /** Lista tarefas ativas ou concluídas */
        get: operations["listTasks"];
        put?: never;
        /** Cria uma tarefa de forma idempotente */
        post: operations["createTask"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/tasks/{taskId}": {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        /** Obtém os detalhes essenciais de uma tarefa */
        get: operations["getTask"];
        /** Substitui atomicamente os campos editáveis */
        put: operations["updateTask"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/tasks/{taskId}/status": {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        get?: never;
        /**
         * Altera uma tarefa entre estados ativos
         * @description Uma tarefa concluída deve ser reaberta antes de receber outro estado ativo.
         */
        put: operations["changeActiveTaskStatus"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/tasks/{taskId}/complete": {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Conclui uma tarefa de forma idempotente */
        post: operations["completeTask"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/tasks/{taskId}/reopen": {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        get?: never;
        put?: never;
        /** Reabre uma tarefa no estado ativo anterior */
        post: operations["reopenTask"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        /**
         * @default active
         * @enum {string}
         */
        TaskView: "active" | "completed";
        /** @enum {string} */
        TaskStatus: "not_started" | "in_progress" | "blocked" | "completed";
        /** @enum {string} */
        ActiveTaskStatus: "not_started" | "in_progress" | "blocked";
        /**
         * @default none
         * @enum {string}
         */
        TaskPriority: "none" | "low" | "medium" | "high" | "urgent";
        Task: {
            /** Format: uuid */
            id: string;
            title: string;
            description: string | null;
            status: components["schemas"]["TaskStatus"];
            priority: components["schemas"]["TaskPriority"];
            /** Format: date */
            dueDate: string | null;
            /** Format: date-time */
            createdAt: string;
            /** Format: date-time */
            updatedAt: string;
            /** Format: date-time */
            completedAt: string | null;
        };
        CreateTaskRequest: {
            title: string;
            /** @default null */
            description: string | null;
            priority?: components["schemas"]["TaskPriority"];
            /**
             * Format: date
             * @default null
             */
            dueDate: string | null;
        };
        UpdateTaskRequest: {
            title: string;
            description: string | null;
            priority: components["schemas"]["TaskPriority"];
            /** Format: date */
            dueDate: string | null;
        };
        ChangeActiveStatusRequest: {
            status: components["schemas"]["ActiveTaskStatus"];
        };
        ProblemDetails: {
            /** Format: uri-reference */
            type: string;
            title: string;
            status: number;
            detail?: string;
            /** Format: uri-reference */
            instance?: string;
            traceId: string;
        } & {
            [key: string]: unknown;
        };
        ValidationProblemDetails: components["schemas"]["ProblemDetails"] & {
            errors: {
                [key: string]: string[];
            };
        };
    };
    responses: {
        /** @description A requisição é inválida; nenhuma alteração foi persistida. */
        ValidationProblem: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["ValidationProblemDetails"];
            };
        };
        /** @description A tarefa não existe. */
        NotFoundProblem: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["ProblemDetails"];
            };
        };
        /** @description Falha inesperada; o traceId permite correlação sem expor conteúdo da tarefa. */
        Problem: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["ProblemDetails"];
            };
        };
    };
    parameters: {
        TaskId: string;
        /** @description UUID estável para uma tentativa lógica de criação. Deve ser reutilizado somente em retries da mesma criação. */
        IdempotencyKey: string;
    };
    requestBodies: never;
    headers: never;
    pathItems: never;
}
export type $defs = Record<string, never>;
export interface operations {
    listTasks: {
        parameters: {
            query?: {
                /** @description A visualização ativa é usada quando o parâmetro é omitido. */
                view?: components["schemas"]["TaskView"];
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Tarefas da visualização solicitada. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"][];
                };
            };
            400: components["responses"]["ValidationProblem"];
            500: components["responses"]["Problem"];
        };
    };
    createTask: {
        parameters: {
            query?: never;
            header: {
                /** @description UUID estável para uma tentativa lógica de criação. Deve ser reutilizado somente em retries da mesma criação. */
                "Idempotency-Key": components["parameters"]["IdempotencyKey"];
            };
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["CreateTaskRequest"];
            };
        };
        responses: {
            /** @description Repetição idempotente; retorna a tarefa criada anteriormente com a mesma chave e conteúdo. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            /** @description Tarefa criada nesta requisição. */
            201: {
                headers: {
                    /** @description Caminho do recurso criado. */
                    Location?: string;
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            400: components["responses"]["ValidationProblem"];
            /** @description A chave de idempotência já foi usada com conteúdo diferente. */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["ProblemDetails"];
                };
            };
            500: components["responses"]["Problem"];
        };
    };
    getTask: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Tarefa encontrada. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            404: components["responses"]["NotFoundProblem"];
            500: components["responses"]["Problem"];
        };
    };
    updateTask: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["UpdateTaskRequest"];
            };
        };
        responses: {
            /** @description Tarefa atualizada. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            400: components["responses"]["ValidationProblem"];
            404: components["responses"]["NotFoundProblem"];
            500: components["responses"]["Problem"];
        };
    };
    changeActiveTaskStatus: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["ChangeActiveStatusRequest"];
            };
        };
        responses: {
            /** @description Estado alterado; se o estado já era o solicitado, nenhum evento é acrescentado. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            400: components["responses"]["ValidationProblem"];
            404: components["responses"]["NotFoundProblem"];
            /** @description A tarefa está concluída e precisa ser reaberta. */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["ProblemDetails"];
                };
            };
            500: components["responses"]["Problem"];
        };
    };
    completeTask: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Tarefa concluída; repetições preservam o primeiro evento e instante da conclusão atual. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            404: components["responses"]["NotFoundProblem"];
            500: components["responses"]["Problem"];
        };
    };
    reopenTask: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                taskId: components["parameters"]["TaskId"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Tarefa reaberta e conclusão atual removida; eventos anteriores permanecem preservados. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Task"];
                };
            };
            404: components["responses"]["NotFoundProblem"];
            /** @description A tarefa não está concluída ou não possui uma transição de conclusão válida. */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["ProblemDetails"];
                };
            };
            500: components["responses"]["Problem"];
        };
    };
}
