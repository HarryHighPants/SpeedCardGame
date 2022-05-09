import { NavigateOptions } from 'react-router-dom'
import { IPos } from '../Interfaces/ICard'

// Clamp number between two values with the following line:
export const clamp = (num: number, min: number, max: number) => Math.min(Math.max(num, min), max)

export const delay = (ms: number) => new Promise((res) => setTimeout(res, ms))

export function map(current: number, in_min: number, in_max: number, out_min: number, out_max: number): number {
    const mapped: number = ((current - in_min) * (out_max - out_min)) / (in_max - in_min) + out_min
    return clamp(mapped, out_min, out_max)
}

export function convertTZ(date: string | Date, tzString: string) {
    return new Date((typeof date === 'string' ? new Date(date) : date).toLocaleString('en-US', { timeZone: tzString }))
}

export const GetOffsetInfo = (ourRect: DOMRect | undefined, draggingCardRect: DOMRect | undefined) => {
    let distance = GetDistanceRect(draggingCardRect, ourRect)
    let overlaps = Overlaps(ourRect, draggingCardRect)
    let delta =
        !draggingCardRect || !ourRect
            ? undefined
            : { X: draggingCardRect.x - ourRect.x, Y: draggingCardRect.y - ourRect.y }
    return { distance, overlaps, delta }
}

export const GetDistance = (pos1: IPos | undefined, pos2: IPos | undefined) => {
    if (!pos1 || !pos2) return Infinity
    let a = pos1.x - pos2.x
    let b = pos1.y - pos2.y
    return Math.sqrt(a * a + b * b)
}

export const GetDistanceRect = (rect1: DOMRect | undefined, rect2: DOMRect | undefined) => {
    if (!rect1 || !rect2) return Infinity
    let a = rect1.x - rect2.x
    let b = rect1.y - rect2.y
    return Math.sqrt(a * a + b * b)
}

export const Overlaps = (rect1: DOMRect | undefined, rect2: DOMRect | undefined): boolean => {
    if (!rect1 || !rect2) return false
    return !(
        rect1.right < rect2.left ||
        rect1.left > rect2.right ||
        rect1.bottom < rect2.top ||
        rect1.top > rect2.bottom
    )
}

export const GetRandomId = () => {
    return GetRandomInt(99999999)
}

export const GetRandomInt = (max: number) => {
    return Math.floor(Math.random() * max)
}
