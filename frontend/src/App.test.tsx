// @vitest-environment jsdom

import { act } from 'react'
import { afterEach, expect, test, vi } from 'vitest'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import App from './App'

function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason: Error) => void
  const promise = new Promise<T>((resolvePromise, rejectPromise) => {
    resolve = resolvePromise
    reject = rejectPromise
  })
  return { promise, resolve, reject }
}

function jsonResponse(body: unknown, status = 200): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => body,
  } as Response
}

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

test.each(['empty result', 'network error'])(
  'an older %s cannot replace the list after creation',
  async (lateResult) => {
    const initialGet = deferred<Response>()
    let getCount = 0
    const fetchMock = vi.fn(async (_input: RequestInfo | URL, init?: RequestInit) => {
      if (init?.method === 'POST') return jsonResponse({}, 201)
      getCount++
      if (getCount === 1) return initialGet.promise
      return jsonResponse([{ id: 'workspace-1', name: 'Engineering', createdAtUtc: '2026-10-28T00:00:00Z' }])
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<App />)
    await waitFor(() => expect(getCount).toBe(1))
    fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'Engineering' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create' }))
    await screen.findByText('Engineering')

    await act(async () => {
      if (lateResult === 'network error') initialGet.reject(new Error('late response'))
      else initialGet.resolve(jsonResponse([]))
    })

    expect(screen.getByText('Engineering')).toBeTruthy()
    expect(screen.queryByText('Could not load workspaces. Try refreshing the page.')).toBeNull()
  },
)

test('renaming a workspace sends PATCH and refreshes the list', async () => {
  let getCount = 0
  const fetchMock = vi.fn(async (_input: RequestInfo | URL, init?: RequestInit) => {
    if (init?.method === 'PATCH') return jsonResponse({ id: 'workspace-1', name: 'Platform' })
    getCount++
    return jsonResponse([{
      id: 'workspace-1',
      name: getCount === 1 ? 'Engineering' : 'Platform',
      createdAtUtc: '2026-10-28T00:00:00Z',
    }])
  })
  vi.stubGlobal('fetch', fetchMock)

  render(<App />)
  fireEvent.click(await screen.findByRole('button', { name: 'Rename Engineering' }))
  fireEvent.change(screen.getByLabelText('Workspace name'), { target: { value: '  Platform  ' } })
  fireEvent.click(screen.getByRole('button', { name: 'Save' }))

  await screen.findByRole('button', { name: 'Rename Platform' })
  expect(fetchMock).toHaveBeenCalledWith('/api/workspaces/workspace-1', expect.objectContaining({
    method: 'PATCH',
    body: JSON.stringify({ name: '  Platform  ' }),
  }))
  expect(getCount).toBe(2)
})

test('a rename validation error keeps the editor open and the old name visible', async () => {
  const fetchMock = vi.fn(async (_input: RequestInfo | URL, init?: RequestInit) => {
    if (init?.method === 'PATCH') {
      return jsonResponse({ errors: { name: ['Name must be between 3 and 80 characters after trimming.'] } }, 400)
    }
    return jsonResponse([{ id: 'workspace-1', name: 'Engineering', createdAtUtc: '2026-10-28T00:00:00Z' }])
  })
  vi.stubGlobal('fetch', fetchMock)

  render(<App />)
  fireEvent.click(await screen.findByRole('button', { name: 'Rename Engineering' }))
  fireEvent.change(screen.getByLabelText('Workspace name'), { target: { value: ' ' } })
  fireEvent.click(screen.getByRole('button', { name: 'Save' }))

  await screen.findByText('Name must be between 3 and 80 characters after trimming.')
  expect((screen.getByLabelText('Workspace name') as HTMLInputElement).value).toBe(' ')
  expect(fetchMock).toHaveBeenCalledTimes(2)
})

test('a rename request failure shows an error and keeps the draft', async () => {
  const fetchMock = vi.fn(async (_input: RequestInfo | URL, init?: RequestInit) => {
    if (init?.method === 'PATCH') return jsonResponse({}, 500)
    return jsonResponse([{ id: 'workspace-1', name: 'Engineering', createdAtUtc: '2026-10-28T00:00:00Z' }])
  })
  vi.stubGlobal('fetch', fetchMock)

  render(<App />)
  fireEvent.click(await screen.findByRole('button', { name: 'Rename Engineering' }))
  fireEvent.change(screen.getByLabelText('Workspace name'), { target: { value: 'Platform' } })
  fireEvent.click(screen.getByRole('button', { name: 'Save' }))

  await screen.findByText('Could not rename workspace. Please try again.')
  expect((screen.getByLabelText('Workspace name') as HTMLInputElement).value).toBe('Platform')
})
