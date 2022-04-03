import {IPos} from "../Interfaces/ICard";

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
