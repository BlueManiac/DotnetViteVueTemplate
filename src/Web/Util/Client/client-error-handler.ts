import { ErrorPayload } from "vite"

export const showErrorOverlay = async (error: unknown) => {
    const ErrorOverlay = customElements.get('vite-error-overlay')

    if (!ErrorOverlay)
        return false

    try {
        const err = await createErrorObject(error)
        const overlay = new ErrorOverlay(err, true)

        clearErrorOverlay()
        document.body.appendChild(overlay)
    
        return true
    }
    catch {}

    return false
}

function clearErrorOverlay() {
    document
        .querySelectorAll('vite-error-overlay')
        .forEach((n: any) => n.close())
  }

const createErrorObject = async (error: unknown): Promise<ErrorPayload["err"]> => {
    if (!(error instanceof Error)) {
        return {
            plugin: 'client-error-handler',
            message: error as string,
            stack: null
        }
    }

    const stackLine = Array.from(parseStackLines(error))[0]

    const sourceMap = stackLine && await getSourceMap(stackLine.url)

    return {
        plugin: 'client-error-handler',
        message: error["message"],
        stack: error["stack"],
        loc: sourceMap && {
            file: sourceMap.file?.replace(/\//g, '\\'),
            line: Number(stackLine?.lineNumber),
            column: Number(stackLine?.columnNumber)
        },
        id: stackLine?.url
    }

    async function getSourceMap(url: string): Promise<{ file: string } | undefined> {
        const code = await fetch(url, { method: 'get' }).then(x => x.text())
    
        const sourceMappingUrlRegex = /\/\/# sourceMappingURL=data:application\/json;base64,(.*)$/
        const match = code.match(sourceMappingUrlRegex)
    
        if (!match)
            return
        
        const base64Data = match[1]
        const sourceMapJSON = atob(base64Data)
    
        return JSON.parse(sourceMapJSON)
    }

    function* parseStackLines(error: Error) {
        const stackLines = error.stack.split('\n')

        for (let i = 1; i < stackLines.length; i++) {
            const match = /at (.+?) \((.+):(\d+):(\d+)\)/.exec(stackLines[i])

            if (!match)
                continue

            const [, functionName, url, lineNumber, columnNumber] = match

            yield {
                functionName,
                url,
                lineNumber,
                columnNumber
            }
        }
    }
}

const openInEditor = async (fileUrl: string, line: number | string, column: number | string) => {
    const importMetaUrl = new URL(import.meta.url)
    const protocol = importMetaUrl?.protocol
    const host = importMetaUrl?.hostname
    const port = importMetaUrl?.port
    const baseUrl = `${protocol}//${host}:${port}`

    const url = new URL(fileUrl)

    await fetch(`${baseUrl}/__open-in-editor?file=${url.pathname.substring(1)}:${line}:${column}`, { mode: 'no-cors', })
}