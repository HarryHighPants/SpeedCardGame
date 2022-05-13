import {convertTZ} from "./Utilities";

export const getResetTime = () => {
    const aestDate = convertTZ(new Date(), 'Australia/Brisbane')
    return new Date(aestDate).setHours(24, 0, 0, 0)
}