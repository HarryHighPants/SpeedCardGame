import { useState, useEffect } from 'react'

function usePersistentState<T>(key: string, defaultValue: T): [T, (value: T) => void] {
    const [state, setState] = useState(() => {
        const storedValue = localStorage.getItem(key)
        return storedValue !== null ? JSON.parse(storedValue) : defaultValue
    })

    useEffect(() => {
        localStorage.setItem(key, JSON.stringify(state))
    }, [key, state])

    return [state, setState]
}

export default usePersistentState
