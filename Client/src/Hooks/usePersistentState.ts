import { useState, useEffect } from 'react'

function usePersistentState<T>(key: string, defaultValue: T, stringify = true): [T, (value: T) => void] {
    const [state, setState] = useState(() => {
        const storedValue = localStorage.getItem(key)
        return storedValue !== null ? (stringify ? JSON.parse(storedValue) : storedValue) : defaultValue
    })

    useEffect(() => {
        localStorage.setItem(key, stringify ? JSON.stringify(state) : state)
    }, [key, state, stringify])

    return [state, setState]
}

export default usePersistentState
