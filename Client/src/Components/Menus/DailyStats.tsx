import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { HiShare } from 'react-icons/hi'
import Popup from '../Popup'
import { IDailyResults } from '../../Interfaces/IDailyResults'
import CelebrateShaker from '../CelebrateShake'
import toast from 'react-hot-toast'
import { convertTZ } from '../../Helpers/Utilities'
import Countdown from 'react-countdown'

const getResetTime = () => {
	const aestDate = convertTZ(new Date(), 'Australia/Brisbane')
	return new Date(aestDate).setHours(24, 0, 0, 0)
}

interface Props {
	connection: signalR.HubConnection | undefined
}

const DailyStats = ({ connection }: Props) => {
	const [dailyResults, setDailyResults] = useState<IDailyResults>()
	const [nextGameDate, setNextGameDate] = useState<number>(() => getResetTime())
	const [myPlayerName, setMyPlayerName] = useState<string>(() =>
		JSON.parse(localStorage.getItem('playerName') ?? `"Player"`)
	)

	useEffect(() => {
		if (!connection) return
		connection.on('UpdateDailyResults', UpdateDailyResults)

		return () => {
			connection.off('UpdateDailyResults', UpdateDailyResults)
		}
	}, [connection])

	const UpdateDailyResults = (data: any) => {
		let dailyResultsData: IDailyResults = JSON.parse(data)
		setDailyResults(dailyResultsData)
	}

	const onShare = () => {
		if (!dailyResults) {
			return
		}
		let outcomeText =
			(dailyResults.PlayerWon ? `👑 beat ` : `☠️ lost against `) +
			`${dailyResults.BotName} by ${dailyResults.LostBy} card${dailyResults.LostBy > 1 ? 's' : ''}`
		let shareText = `${outcomeText}\n♦️speed.harryab.com`
		navigator.clipboard.writeText(shareText)
		toast.success('Share text copied to clipboard!')
	}

	if (!dailyResults) {
		return <></>
	}

	return (
		<Popup id={'DailyStatsPopup'} onHomeButton={true} customZIndex={65}>
			<h5 style={{ marginTop: 30, marginBottom: -5 }}>Statistics</h5>
			<div style={{ display: 'flex', justifyContent: 'center', alignItems: 'end', marginBottom: 30 }}>
				<Stat value={dailyResults.DailyWins.toString()} description="Wins" />
				<Stat value={dailyResults.DailyLosses.toString()} description="Losses" />
				<Stat value={dailyResults.DailyWinStreak.toString()} description="Streak" />
				<Stat value={dailyResults.MaxDailyWinStreak.toString()} description="Best streak" />
			</div>

			<h5
				style={{
					marginTop: 40,
					marginBottom: -10,
				}}
			>
				Winner is:
			</h5>
			<div style={{ display: 'flex', justifyContent: 'center', alignItems: 'end', marginBottom: 50 }}>
				<CelebrateShaker />
				<h2 style={{ margin: '0 25px' }}>{dailyResults.PlayerWon ? myPlayerName : dailyResults.BotName}</h2>
				<CelebrateShaker startDelay={0.2} />
			</div>

			<div style={{ display: 'flex', justifyContent: 'center', marginBottom: -10 }}>
				<Stat
					value={
						<Countdown
							date={nextGameDate}
							zeroPadTime={2}
							daysInHours
						/>
					}
					description="Next opponent"
				/>
				<BottomButton style={{ marginTop: 25 }} onClick={() => onShare()}>
					Share
					<HiShare style={{ marginBottom: -2, marginLeft: 5 }} />
				</BottomButton>
			</div>
		</Popup>
	)
}

interface StatProps {
	value: string | JSX.Element
	description: string
}
const Stat = ({ value, description }: StatProps): JSX.Element => {
	return (
		<StatWrapper>
			<StatValue>{value}</StatValue>
			<StatDescription>{description}</StatDescription>
		</StatWrapper>
	)
}

const StatValue = styled.h2`
	margin: 0;
`

const StatDescription = styled.p`
	white-space: nowrap;
	margin: 0;
`

const StatWrapper = styled.div`
	display: flex;
	flex-direction: column;
	align-items: center;
	margin: 15px;
`

const BottomButton = styled.button`
	height: 30px;
	padding: 0 10px;
	margin: 25px 5px 5px;
`

export default DailyStats
