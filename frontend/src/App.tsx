import { useEffect, useRef, useState, type FormEvent } from 'react'

type Workspace = {
  id: string
  name: string
  createdAtUtc: string
  version: number
}

type ValidationProblem = {
  errors?: Record<string, string[]>
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export default function App() {
  const [name, setName] = useState('')
  const [workspaces, setWorkspaces] = useState<Workspace[]>([])
  const [loading, setLoading] = useState(true)
  const [listError, setListError] = useState('')
  const [formError, setFormError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editingVersion, setEditingVersion] = useState<number | null>(null)
  const [draftName, setDraftName] = useState('')
  const [editError, setEditError] = useState('')
  const [conflictMessage, setConflictMessage] = useState('')
  const [renaming, setRenaming] = useState(false)
  const latestListRequest = useRef(0)

  async function loadWorkspaces() {
    const requestId = ++latestListRequest.current
    setLoading(true)
    setListError('')
    try {
      const response = await fetch(`${apiBaseUrl}/api/workspaces`)
      if (!response.ok) throw new Error('Could not load workspaces.')
      const items = (await response.json()) as Workspace[]
      if (requestId === latestListRequest.current) setWorkspaces(items)
    } catch {
      if (requestId === latestListRequest.current) {
        setListError('Could not load workspaces. Try refreshing the page.')
      }
    } finally {
      if (requestId === latestListRequest.current) setLoading(false)
    }
  }

  useEffect(() => {
    void loadWorkspaces()
    return () => { latestListRequest.current++ }
  }, [])

  async function createWorkspace(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError('')
    setConflictMessage('')
    setSubmitting(true)
    try {
      const response = await fetch(`${apiBaseUrl}/api/workspaces`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      if (response.status === 400) {
        const problem = (await response.json()) as ValidationProblem
        setFormError(problem.errors?.name?.[0] ?? 'Enter a valid workspace name.')
        return
      }
      if (!response.ok) throw new Error('Could not create workspace.')
      setName('')
      await loadWorkspaces()
    } catch {
      setFormError('Could not create workspace. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  async function renameWorkspace(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (editingId === null || editingVersion === null) return
    setEditError('')
    setRenaming(true)
    try {
      const response = await fetch(`${apiBaseUrl}/api/workspaces/${editingId}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: draftName, expectedVersion: editingVersion }),
      })
      if (response.status === 400) {
        const problem = (await response.json()) as ValidationProblem
        setEditError(problem.errors?.name?.[0] ?? 'Enter a valid workspace name.')
        return
      }
      if (response.status === 409) {
        setEditingId(null)
        setEditingVersion(null)
        await loadWorkspaces()
        setConflictMessage('This workspace changed elsewhere. Review its current name and try again.')
        return
      }
      if (!response.ok) throw new Error('Could not rename workspace.')
      setEditingId(null)
      setEditingVersion(null)
      await loadWorkspaces()
    } catch {
      setEditError('Could not rename workspace. Please try again.')
    } finally {
      setRenaming(false)
    }
  }

  return (
    <main className="container">
      <header>
        <span className="eyebrow">Conflux</span>
        <h1>Workspaces</h1>
        <p>Create a space and find it here whenever you return.</p>
      </header>

      <section aria-labelledby="create-heading" className="panel">
        <h2 id="create-heading">Create workspace</h2>
        <form onSubmit={(event) => void createWorkspace(event)}>
          <label htmlFor="workspace-name">Name</label>
          <div className="form-row">
            <input
              id="workspace-name"
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="Engineering"
              maxLength={100}
              aria-describedby={formError ? 'form-error' : undefined}
            />
            <button disabled={submitting} type="submit">
              {submitting ? 'Creating…' : 'Create'}
            </button>
          </div>
          {formError && <p id="form-error" className="error" role="alert">{formError}</p>}
        </form>
      </section>

      <section aria-labelledby="list-heading" className="panel">
        <h2 id="list-heading">Your workspaces</h2>
        {conflictMessage && <p className="error" role="alert">{conflictMessage}</p>}
        {loading ? (
          <p role="status">Loading workspaces…</p>
        ) : listError ? (
          <p className="error" role="alert">{listError}</p>
        ) : workspaces.length === 0 ? (
          <p>No workspaces yet. Create your first one above.</p>
        ) : (
          <ul>
            {workspaces.map((workspace) => (
              <li key={workspace.id}>
                {editingId === workspace.id ? (
                  <form onSubmit={(event) => void renameWorkspace(event)}>
                    <label htmlFor={`rename-${workspace.id}`}>Workspace name</label>
                    <div className="form-row">
                      <input
                        id={`rename-${workspace.id}`}
                        value={draftName}
                        onChange={(event) => setDraftName(event.target.value)}
                        maxLength={100}
                        aria-describedby={editError ? 'rename-error' : undefined}
                      />
                      <button disabled={renaming} type="submit">{renaming ? 'Saving…' : 'Save'}</button>
                      <button disabled={renaming} type="button" onClick={() => {
                        setEditingId(null)
                        setEditingVersion(null)
                      }}>Cancel</button>
                    </div>
                    {editError && <p id="rename-error" className="error" role="alert">{editError}</p>}
                  </form>
                ) : (
                  <>
                    <strong>{workspace.name}</strong>
                    <button disabled={renaming} type="button" onClick={() => {
                      setEditingId(workspace.id)
                      setEditingVersion(workspace.version)
                      setDraftName(workspace.name)
                      setEditError('')
                      setConflictMessage('')
                    }}>Rename {workspace.name}</button>
                  </>
                )}
                <time dateTime={workspace.createdAtUtc}>
                  {new Date(workspace.createdAtUtc).toLocaleDateString()}
                </time>
              </li>
            ))}
          </ul>
        )}
      </section>
    </main>
  )
}
