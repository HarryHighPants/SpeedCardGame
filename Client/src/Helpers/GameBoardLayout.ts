import { IPos } from '../Interfaces/ICard'

class GameBoardLayout {
  public static maxWidth = 1200

  private static centerPilesPadding = 0.08
  private static playerHeightPadding = 0.25
  private static playerCardSeperation = 0.1
  private static playerHandCardsCenterX = 0.3
  private static playerKittyCenterX = 0.8


  static GetHandCardPositions(playerIndex: number): IPos[] {
        let cardPositions = Array(5).fill(0).map((e, i) => {
            return {
                x: this.playerHandCardsCenterX + this.playerCardSeperation * (i - 2),
                y: 1 - this.playerHeightPadding,
            } as IPos
        })

        if (playerIndex == 1) {
            cardPositions = this.FlipPositions(cardPositions)
        }
        return cardPositions
    }

  static GetKittyCardPosition(playerIndex: number): IPos {
        let cardPosition = { x: this.playerKittyCenterX, y: 1 - this.playerHeightPadding } as IPos

        if (playerIndex == 1) {
            cardPosition = this.FlipPositions([cardPosition])[0]
        }
        return cardPosition
    }

  static GetCenterCardPositions(): IPos[] {
    return Array(2).fill(0).map((e, i) => {
            return { x: 0.5 + this.centerPilesPadding * (i && -1), y: 0.5 } as IPos
        })
    }

  static FlipPositions(positions: IPos[]): IPos[] {
        return positions.map((c) => {
            return { x: Math.abs(c.x - 1), y: Math.abs(c.y - 1) } as IPos
        })
    }
}

export default GameBoardLayout
