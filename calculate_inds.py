import pandas_ta as ta
import pandas as pd

FILE_PATH = "./data/btc_live_data_15m.csv"
DEST_FP   = "./data/btc_live_data_15m_inds.csv" 

def main():
    df = pd.read_csv(FILE_PATH)
    bb : pd.DataFrame = ta.bbands(df['Close'], length=20, std=2)
    df['ema_20'] = ta.ema(df['Close'], 20).round(2)
    df['rsi_14'] = ta.rsi(df['Close'], 14).round(2)
    df['bb_bbm'] = bb['BBM_20_2.0'].round(2)
    df['bb_bbh'] = bb['BBU_20_2.0'].round(2)
    df['bb_bbl'] = bb['BBL_20_2.0'].round(2)
    df.to_csv(DEST_FP, index=False)

if __name__ == '__main__':
    main()

