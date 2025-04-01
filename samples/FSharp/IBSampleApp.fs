open System
open IBApi

type IBClient() as this =
    let mutable nextOrderId = 0
    let signal = new EReaderMonitorSignal()
    let clientSocket = new EClientSocket(this :> EWrapper, signal)

    // Initialize reader
    let mutable reader = Unchecked.defaultof<EReader>

    interface EWrapper with
        // Error handlers
        member _.error(e: exn) = 
            printfn "Error: %s" e.Message
        
        member _.error(str: string) = 
            printfn "Error: %s" str
            
        member _.error(id: int, errorCode: int, errorMsg: string, advancedOrderRejectJson: string) =
            printfn "Error: Id=%d, Code=%d, Msg=%s" id errorCode errorMsg

        // Connection-related methods
        member _.connectionClosed() =
            printfn "Connection closed"

        // Tick-related methods
        member _.tickPrice(reqId: int, field: int, price: double, attrib: TickAttrib) =
            printfn "Tick Price: ReqId=%d, Field=%d, Price=%.2f" reqId field price
        member _.tickSize(_: int, _: int, _: decimal) = ()
        member _.tickString(_: int, _: int, _: string) = ()
        member _.tickGeneric(_: int, _: int, _: double) = ()
        member _.tickEFP(tickerId, tickType, basisPoints, formattedBasisPoints, impliedFuture, 
                        holdDays, futureLastTradeDate, dividendImpact, dividendsToLastTradeDate) = ()
        member _.tickOptionComputation(tickerId, field, tickAttrib, impliedVolatility, delta,
                                     optPrice, pvDividend, gamma, vega, theta, undPrice) = ()
        member _.tickSnapshotEnd(tickerId) = ()

        // Account-related methods
        member _.updateAccountValue(key, value, currency, accountName) = ()
        member _.updatePortfolio(contract, position, marketPrice, marketValue, averageCost,
                               unrealizedPNL, realizedPNL, accountName) = ()
        member _.updateAccountTime(timestamp) = ()
        member _.accountDownloadEnd(accountName) = ()
        member _.accountSummary(reqId, account, tag, value, currency) = ()
        member _.accountSummaryEnd(reqId) = ()
        member _.managedAccounts(accountsList) = ()

        // Contract-related methods
        member _.bondContractDetails(reqId, contractDetails) = ()
        member _.contractDetails(reqId, contractDetails) = ()
        member _.contractDetailsEnd(reqId) = ()
        member _.symbolSamples(reqId, contractDescriptions) = ()

        // Market data methods
        member _.marketDataType(reqId: int, marketDataType: int) = ()
        member _.realtimeBar(reqId: int, time: int64, open_: double, high: double, 
                            low: double, close: double, volume: decimal, wap: decimal, count: int) = ()
        member _.historicalData(reqId: int, bar: Bar) = 
            printfn "Historical Data: ReqId=%i, Time=%s, Open=%.2f, High=%.2f, Low=%.2f, Close=%.2f, Volume=%.2f, WAP=%.2f, Count=%d" 
                reqId 
                bar.Time 
                bar.Open 
                bar.High 
                bar.Low 
                bar.Close 
                bar.Volume 
                bar.WAP 
                bar.Count
        
        member _.historicalDataEnd(reqId: int, start: string, e: string) = 
            printfn "Historical Data End: ReqId=%d, Start=%s, End=%s" reqId start e

        member _.historicalDataUpdate(reqId: int, bar: Bar) = 
            printfn "Historical Data Update: ReqId=%d, Time=%s, Open=%.2f, High=%.2f, Low=%.2f, Close=%.2f" 
                reqId bar.Time bar.Open bar.High bar.Low bar.Close

        // Historical tick methods
        member _.historicalTicks(reqId: int, ticks: HistoricalTick[], d: bool) =
            printfn "Historical Ticks: ReqId=%d, Count=%d, Done=%b" reqId ticks.Length d
            for tick in ticks do
                printfn "\tTime=%d, Price=%.2f, Size=%M" 
                    tick.Time tick.Price tick.Size

        member _.historicalTicksBidAsk(reqId: int, ticks: HistoricalTickBidAsk[], d: bool) =
            printfn "Historical Ticks Bid/Ask: ReqId=%d, Count=%d, Done=%b" reqId ticks.Length d
            for tick in ticks do
                printfn "\tTime=%d, Bid=%.2f, Ask=%.2f, BidSize=%M, AskSize=%M" 
                    tick.Time tick.PriceBid tick.PriceAsk tick.SizeBid tick.SizeAsk

        member _.historicalTicksLast(reqId: int, ticks: HistoricalTickLast[], d: bool) =
            printfn "Historical Ticks Last: ReqId=%d, Count=%d, Done=%b" reqId ticks.Length d
            for tick in ticks do
                printfn "\tTime=%d, Price=%.2f, Size=%M, Exchange=%s, Special=%s" 
                    tick.Time tick.Price tick.Size tick.Exchange tick.SpecialConditions

        // Tick-by-tick methods
        member _.tickByTickAllLast(reqId: int, tickType: int, time: int64, price: float, 
                                 size: decimal, tickAttribLast: TickAttribLast, 
                                 exchange: string, specialConditions: string) =
            printfn "Tick-By-Tick All Last: ReqId=%d, Type=%d, Time=%d, Price=%.2f, Size=%M, Exchange=%s" 
                reqId tickType time price size exchange

        member _.tickByTickBidAsk(reqId: int, time: int64, bidPrice: float, askPrice: float, 
                                 bidSize: decimal, askSize: decimal, 
                                 tickAttribBidAsk: TickAttribBidAsk) =
            printfn "Tick-By-Tick Bid/Ask: ReqId=%d, Time=%d, Bid=%.2f, Ask=%.2f, BidSize=%M, AskSize=%M" 
                reqId time bidPrice askPrice bidSize askSize

        member _.tickByTickMidPoint(reqId: int, time: int64, midPoint: float) =
            printfn "Tick-By-Tick MidPoint: ReqId=%d, Time=%d, MidPoint=%.2f" 
                reqId time midPoint

        // Position-related methods
        member _.position(account: string, contract: Contract, pos: decimal, avgCost: double) = ()
        member _.positionEnd() = ()

        // Order-related methods
        member _.orderStatus(orderId: int, status: string, filled: decimal, remaining: decimal, 
                           avgFillPrice: double, permId: int, parentId: int, lastFillPrice: double, 
                           clientId: int, whyHeld: string, mktCapPrice: double) = 
            printfn "Order Status: Id=%d, Status=%s, Filled=%M, Remaining=%M, AvgFillPrice=%.2f" 
                orderId status filled remaining avgFillPrice

        member _.openOrder(orderId: int, contract: Contract, order: Order, orderState: OrderState) = 
            printfn "Open Order: Id=%d, Symbol=%s, Action=%s, Type=%s, Quantity=%M, Price=%.2f" 
                orderId contract.Symbol order.Action order.OrderType order.TotalQuantity order.LmtPrice

        member _.openOrderEnd() = 
            printfn "Open Order End"

        member _.completedOrder(contract: Contract, order: Order, orderState: OrderState) = 
            printfn "Completed Order: Symbol=%s, Action=%s, Type=%s, Quantity=%M, Price=%.2f" 
                contract.Symbol order.Action order.OrderType order.TotalQuantity order.LmtPrice

        member _.completedOrdersEnd() = 
            printfn "Completed Orders End"

        member _.orderBound(orderId: int64, apiClientId: int, apiOrderId: int) =
            printfn "Order Bound: OrderId=%d, ApiClientId=%d, ApiOrderId=%d" orderId apiClientId apiOrderId

        // Execution-related methods
        member _.execDetails(reqId: int, contract: Contract, execution: Execution) = 
            printfn "Execution Details: ReqId=%d, Symbol=%s, Side=%s, Shares=%M, Price=%.2f" 
                reqId contract.Symbol execution.Side execution.Shares execution.Price

        member _.execDetailsEnd(reqId: int) = 
            printfn "Execution Details End: ReqId=%d" reqId

        member _.commissionReport(commissionReport: CommissionReport) = 
            printfn "Commission Report: Commission=%.2f, Currency=%s" 
                commissionReport.Commission commissionReport.Currency

        // Market depth methods
        member _.updateMktDepth(tickerId: int, position: int, operation: int, side: int, 
                              price: float, size: decimal) = 
            printfn "Market Depth: TickerId=%d, Position=%d, Operation=%d, Side=%d, Price=%.2f, Size=%M" 
                tickerId position operation side price size

        member _.updateMktDepthL2(tickerId: int, position: int, marketMaker: string, 
                                operation: int, side: int, price: float, size: decimal, 
                                isSmartDepth: bool) = 
            printfn "Market Depth L2: TickerId=%d, Position=%d, MM=%s, Operation=%d, Side=%d, Price=%.2f, Size=%M, Smart=%b" 
                tickerId position marketMaker operation side price size isSmartDepth

        // Market depth exchange methods
        member _.mktDepthExchanges(depthMktDataDescriptions: DepthMktDataDescription[]) =
            printfn "Market Depth Exchanges received: Count=%d" depthMktDataDescriptions.Length
            for desc in depthMktDataDescriptions do
                printfn "\tExchange: %s, Security Type: %s, Listing: %s" 
                    desc.Exchange desc.SecType desc.ListingExch

        // News bulletin methods
        member _.updateNewsBulletin(msgId: int, msgType: int, message: string, origExchange: string) = 
            printfn "News Bulletin: Id=%d, Type=%d, Message=%s, Exchange=%s" 
                msgId msgType message origExchange

        // News-related methods
        member _.tickNews(tickerId: int, timeStamp: int64, providerCode: string, 
                         articleId: string, headline: string, extraData: string) =
            printfn "News Tick: Id=%d, Time=%d, Provider=%s, Article=%s, Headline=%s" 
                tickerId timeStamp providerCode articleId headline

        member _.newsArticle(requestId: int, articleType: int, articleText: string) =
            printfn "News Article: ReqId=%d, Type=%d, Text=%s" requestId articleType articleText

        member _.historicalNews(requestId: int, time: string, providerCode: string,
                              articleId: string, headline: string) =
            printfn "Historical News: ReqId=%d, Time=%s, Provider=%s, Article=%s, Headline=%s" 
                requestId time providerCode articleId headline

        member _.historicalNewsEnd(requestId: int, hasMore: bool) =
            printfn "Historical News End: ReqId=%d, HasMore=%b" requestId hasMore

        member _.historicalSchedule(reqId: int, startDateTime: string, endDateTime: string, timeZone: string, sessions: HistoricalSession[]) =
            printfn "Historical Schedule: ReqId=%d, StartDateTime=%s, EndDateTime=%s, TimeZone=%s, Sessions=%d" 
                reqId startDateTime endDateTime timeZone sessions.Length

        // Market data-related methods
        member _.tickReqParams(tickerId: int, minTick: float, bboExchange: string, 
                             snapshotPermissions: int) =
            printfn "Tick Request Params: Id=%d, MinTick=%.2f, Exchange=%s, Permissions=%d" 
                tickerId minTick bboExchange snapshotPermissions

        member _.headTimestamp(reqId: int, headTimestamp: string) =
            printfn "Head Timestamp: ReqId=%d, Timestamp=%s" reqId headTimestamp

        member _.histogramData(reqId: int, data: HistogramEntry[]) =
            printfn "Histogram Data: ReqId=%d, Entries=%d" reqId data.Length
            for entry in data do
                printfn "\tPrice=%.2f, Size=%f" entry.Price entry.Size

        // Market data rerouting methods
        member _.rerouteMktDataReq(reqId: int, conId: int, exchange: string) =
            printfn "Reroute Market Data Request: ReqId=%d, ConId=%d, Exchange=%s" 
                reqId conId exchange

        member _.rerouteMktDepthReq(reqId: int, conId: int, exchange: string) =
            printfn "Reroute Market Depth Request: ReqId=%d, ConId=%d, Exchange=%s" 
                reqId conId exchange

        // Financial Advisor methods
        member _.receiveFA(faDataType: int, faXmlData: string) = 
            printfn "FA Data: Type=%d, Data=%s" faDataType faXmlData

        member _.replaceFAEnd(reqId: int, text: string) =
            printfn "Replace FA End: ReqId=%d, Text=%s" reqId text

        // API verification methods
        member _.verifyMessageAPI(apiData: string) = 
            printfn "Verify Message API: %s" apiData

        member _.verifyCompleted(isSuccessful: bool, errorText: string) = 
            printfn "Verify Completed: Success=%b, Error=%s" isSuccessful errorText

        // Authentication and verification methods
        member _.verifyAndAuthMessageAPI(apiData: string, xyzChallenge: string) = 
            printfn "Verify And Auth Message API: Data=%s, Challenge=%s" apiData xyzChallenge

        member _.verifyAndAuthCompleted(isSuccessful: bool, errorText: string) = 
            printfn "Verify And Auth Completed: Success=%b, Error=%s" isSuccessful errorText

        member _.connectAck() = 
            printfn "Connection Acknowledged"

        // Group display methods
        member _.displayGroupList(reqId: int, groups: string) = 
            printfn "Display Group List: ReqId=%d, Groups=%s" reqId groups

        member _.displayGroupUpdated(reqId: int, contractInfo: string) = 
            printfn "Display Group Updated: ReqId=%d, ContractInfo=%s" reqId contractInfo

        // Multi-position methods
        member _.positionMulti(requestId: int, account: string, modelCode: string, 
                             contract: Contract, pos: decimal, avgCost: float) = 
            printfn "Position Multi: ReqId=%d, Account=%s, Model=%s, Symbol=%s, Pos=%M, AvgCost=%.2f" 
                requestId account modelCode contract.Symbol pos avgCost

        member _.positionMultiEnd(requestId: int) = 
            printfn "Position Multi End: ReqId=%d" requestId

        // Multi-account update methods
        member _.accountUpdateMulti(requestId: int, account: string, modelCode: string, 
                                  key: string, value: string, currency: string) = 
            printfn "Account Update Multi: ReqId=%d, Account=%s, Model=%s, Key=%s, Value=%s, Currency=%s" 
                requestId account modelCode key value currency

        member _.accountUpdateMultiEnd(requestId: int) = 
            printfn "Account Update Multi End: ReqId=%d" requestId

        // Family codes method
        member _.familyCodes(familyCodes: FamilyCode[]) = 
            printfn "Family Codes received: Count=%d" familyCodes.Length
            for code in familyCodes do
                printfn "\tAccount ID: %s, Family Code: %s" 
                    code.AccountID 
                    code.FamilyCodeStr  // Changed from FamilyCode to FamilyCodeStr

        // Websocket methods
        member _.wshMetaData(reqId: int, dataJson: string) =
            printfn "WSH Meta Data: ReqId=%d, Data=%s" reqId dataJson

        member _.wshEventData(reqId: int, dataJson: string) =
            printfn "WSH Event Data: ReqId=%d, Data=%s" reqId dataJson

        // User info method
        member _.userInfo(reqId: int, whiteBrandingId: string) =
            printfn "User Info: ReqId=%d, WhiteBrandingId=%s" reqId whiteBrandingId

        // Other required methods
        member _.nextValidId(orderId) = 
            nextOrderId <- orderId
            printfn "Next Valid Id: %d" orderId
        
        member _.currentTime(time: int64) = ()

        member _.deltaNeutralValidation(reqId: int, deltaNeutralContract: DeltaNeutralContract) = ()
        member _.fundamentalData(reqId: int, data: string) = ()
        member _.marketRule(marketRuleId: int, priceIncrements: PriceIncrement[]) = ()  // Changed to array
        member _.pnl(reqId: int, dailyPnL: double, unrealizedPnL: double, realizedPnL: double) = ()
        member _.pnlSingle(reqId, pos, dailyPnL: double, unrealizedPnL: double, realizedPnL: double, value: double) = ()
        member _.scannerData(reqId: int, rank: int, contractDetails: ContractDetails, distance: string, 
                           benchmark: string, projection: string, legsStr: string) = ()
        member _.scannerDataEnd(reqId: int) = ()
        member _.scannerParameters(xml: string) = ()
        member _.securityDefinitionOptionParameter(reqId: int, exchange: string, underlyingConId: int, 
                                                tradingClass: string, multiplier: string, 
                                                expirations: System.Collections.Generic.HashSet<string>, 
                                                strikes: System.Collections.Generic.HashSet<double>) = ()
        member _.securityDefinitionOptionParameterEnd(reqId: int) = ()
        member _.smartComponents(reqId: int, theMap) = ()
        member _.softDollarTiers(reqId: int, tiers) = ()
        member _.newsProviders(newsProviders) = ()

    member this.Connect() =
        clientSocket.eConnect("127.0.0.1", 7496, 0)
        
        // Initialize and start the message reader thread
        if clientSocket.IsConnected() then
            reader <- new EReader(clientSocket, signal)
            reader.Start()
            
            // Create a reader thread
            async {
                while clientSocket.IsConnected() do
                    signal.waitForSignal() |> ignore
                    reader.processMsgs() |> ignore
            } |> Async.Start
            
        while not <| clientSocket.IsConnected() do
            System.Threading.Thread.Sleep(100)
        printfn "Connected to TWS"

    member this.Disconnect() =
        clientSocket.eDisconnect()
        printfn "Disconnected from TWS"

    member this.RequestMarketData(symbol: string) =
        let contract = Contract()
        contract.Symbol <- symbol
        contract.SecType <- "STK"
        contract.Exchange <- "SMART"
        contract.Currency <- "USD"

        let mktDataOptions = System.Collections.Generic.List<TagValue>()
        clientSocket.reqMktData(1, contract, "", false, false, mktDataOptions)
        printfn "Requested market data for %s" symbol

    member this.RequestHistoricalData(symbol: string) =
        let contract = Contract()
        contract.Symbol <- symbol
        contract.SecType <- "STK"
        contract.Exchange <- "SMART"
        contract.Currency <- "USD"

        let endDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss")
        let duration = "1 D"  // 1 day
        let barSize = "1 min"  // 1 minute bars
        let whatToShow = "TRADES"
        let useRTH = 1  // Regular Trading Hours only
        let formatDate = 1
        let keepUpToDate = false
        let chartOptions = System.Collections.Generic.List<TagValue>()

        clientSocket.reqHistoricalData(2, contract, endDateTime, duration, barSize, 
                                     whatToShow, useRTH, formatDate, keepUpToDate, chartOptions)
        printfn "Requested historical data for %s" symbol

    member this.PlaceOrder(symbol: string, quantity: int, price: decimal) =
        let contract = Contract()
        contract.Symbol <- symbol
        contract.SecType <- "STK"
        contract.Exchange <- "SMART"
        contract.Currency <- "USD"

        let order = Order()
        order.Action <- "BUY"
        order.OrderType <- "LMT"
        order.TotalQuantity <- decimal quantity  // Convert to float as required by IB API
        order.LmtPrice <- float price  // Convert decimal price to float
        order.OrderId <- nextOrderId

        clientSocket.placeOrder(nextOrderId, contract, order)
        printfn "Placed order for %d shares of %s at $%.2M" quantity symbol price
        nextOrderId <- nextOrderId + 1

[<EntryPoint>]
let main argv =
    let client = IBClient()
    client.Connect()

    // Request market data for Google (symbol: GOOGL)
    client.RequestMarketData("GOOGL")

    // Wait for a few seconds to receive market data
    System.Threading.Thread.Sleep(5000)

    // Request historical data for Google (symbol: GOOGL)
    client.RequestHistoricalData("GOOGL")

    // Place a limit order to buy 10 shares of Google at $100
    // client.PlaceOrder("GOOGL", 10, decimal 100.0)

    // Wait for a few seconds before canceling the order
    System.Threading.Thread.Sleep(5000)

    client.Disconnect()
    0
