﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Globalization;

namespace LiveResults.Client
{
    public class OlaParser : IExternalSystemResultParser
    {
        private IDbConnection m_Connection;
        private int m_EventID;
        private int m_EventRaceId;

        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_Continue = false;
        public OlaParser(IDbConnection conn, int eventID, int eventRaceId)
        {
            m_Connection = conn;
            m_EventID = eventID;
            m_EventRaceId = eventRaceId;
        }

        //private void FireOnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
        private void FireOnResult(Result newResult)
        {
            if (OnResult != null)
            {
                OnResult(newResult);
            }
        }
        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }

        System.Threading.Thread th;

        public void Start()
        {
            m_Continue = true;
            th = new System.Threading.Thread(new System.Threading.ThreadStart(run));
            th.Start();
        }

        public void Stop()
        {
            m_Continue = false;
        }

        private void run()
        {
            while (m_Continue)
            {
                try
                {
                    if (m_Connection.State != System.Data.ConnectionState.Open)
                    {
                        if (m_Connection is System.Data.H2.H2Connection)
                        {
                            (m_Connection as System.Data.H2.H2Connection).Open("root", "");
                        }
                        else
                        {
                            m_Connection.Open();
                        }
                    }

                    string paramOper = "?";
                    if (m_Connection is MySql.Data.MySqlClient.MySqlConnection)
                    {
                        paramOper = "?date";
                    }

                    /*Detect eventtype*/

                    string scmd = "select eventForm from events where eventid = " + m_EventID;
                    IDbCommand cmd = m_Connection.CreateCommand();
                    cmd.CommandText = scmd;

                    string form = cmd.ExecuteScalar() as string;
                    bool isRelay = false;

                    if (form.ToLower().Contains("relay"))
                        isRelay = true;

                    /* detect ola version*/
                    scmd = "select versionNumber from Version";
                    cmd = m_Connection.CreateCommand();
                    cmd.CommandText = scmd;

                    int version = Convert.ToInt32(cmd.ExecuteScalar());

                    bool isOla5 = version >= 500;


                    string baseCommand = "select results.modifyDate, results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, clubs.name as clubname, eventclasses.shortName, results.runnerStatus, results.entryid, results.allocatedStartTime, results.starttime from results, entries, Persons, Clubs, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.clubid = clubs.clubid and results.runnerStatus != 'notActivated'  and results.modifyDate > " + paramOper;
                    string splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.entryid, results.allocatedStartTime, persons.familyname as lastname, persons.firstname as firstname, clubs.name as clubname, eventclasses.shortName, splittimes.passedCount from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, Clubs, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_EventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.clubid = clubs.clubid and splitTimes.modifyDate > " + paramOper;
                    if (isOla5)
                    {
                        baseCommand = "select results.modifyDate, results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, organisations.shortname as clubname, eventclasses.shortName, results.runnerStatus, results.entryid, results.allocatedStartTime, results.starttime, entries.allocationControl, entries.allocationEntryId from results, entries, Persons, organisations, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.defaultorganisationid = organisations.organisationid and raceClasses.raceClassStatus <> 'notUsed' and results.modifyDate > " + paramOper;
                        splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.entryid, results.allocatedStartTime, results.starttime, persons.familyname as lastname, persons.firstname as firstname, organisations.name as clubname, eventclasses.shortName, splittimes.passedCount,entries.allocationControl, entries.allocationEntryId from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, organisations, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_EventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.defaultorganisationid = organisations.organisationid and raceClasses.raceClassStatus <> 'notUsed' and  splitTimes.modifyDate > " + paramOper;
                    }

                    if (isRelay)
                    {
                        baseCommand = "select results.modifyDate,results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, entries.teamName as clubname, eventclasses.shortName, raceclasses.relayleg, results.runnerStatus, results.resultId as entryId, results.finishTime, results.allocatedStartTime, results.starttime, (select firststarttime from raceclasses rc where rc.eventClassId = eventclasses.EventClassID and rc.relayleg=1 and rc.eventRaceId = " + m_EventRaceId + ") as firststarttime from results, entries, Persons, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and results.relaypersonid = persons.personid and raceClasses.raceClassStatus <> 'notUsed' and  results.modifyDate > " + paramOper;
                        splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.resultId as entryId, results.allocatedStartTime, persons.familyname as lastname, persons.firstname as firstname, entries.teamName as clubname, eventclasses.shortName,raceclasses.relayleg, splittimes.passedCount,results.allocatedStartTime, (select firststarttime from raceclasses rc where rc.eventClassId = eventclasses.EventClassID and rc.relayleg=1 and rc.eventRaceId = " + m_EventRaceId + ") as firststarttime  from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_EventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and results.relaypersonid = persons.personid and raceClasses.raceClassStatus <> 'notUsed' and splitTimes.modifyDate > " + paramOper;
                    }
                    
                    cmd.CommandText = baseCommand; //new OleDbCommand(baseCommand, m_Connection);
                    IDbCommand cmdSplits = m_Connection.CreateCommand();// new OleDbCommand(splitbaseCommand, m_Connection);
                    cmdSplits.CommandText = splitbaseCommand;
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = "date";
                    if (m_Connection is MySql.Data.MySqlClient.MySqlConnection || m_Connection is System.Data.H2.H2Connection)
                    {
                        param.DbType = DbType.String;
                        param.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        param.DbType = DbType.DateTime;
                        param.Value = DateTime.Now;
                    }
                    

                    IDbDataParameter splitparam = cmdSplits.CreateParameter();
                    splitparam.ParameterName = "date";
                    if (m_Connection is MySql.Data.MySqlClient.MySqlConnection || m_Connection is System.Data.H2.H2Connection)
                    {
                        splitparam.DbType = DbType.String;
                        splitparam.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        splitparam.DbType = DbType.DateTime;
                        splitparam.Value = DateTime.Now;
                    }
                   
                    DateTime lastDateTime = DateTime.Now.AddMonths(-120);
                    DateTime lastSplitDateTime = DateTime.Now.AddMonths(-120);
                    param.Value = lastDateTime;
                    splitparam.Value = lastSplitDateTime;

                    cmd.Parameters.Add(param);
                    cmdSplits.Parameters.Add(splitparam);

                    FireLogMsg("OLA Monitor thread started");
                    IDataReader reader = null;
                    Dictionary<int, RunnerPair> runnerPairs = new Dictionary<int, RunnerPair>();
                    while (m_Continue)
                    {
                        string lastRunner = "";
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            if (cmd is MySql.Data.MySqlClient.MySqlCommand || m_Connection is System.Data.H2.H2Connection)
                            {
                                (cmd.Parameters["date"] as IDbDataParameter).Value = lastDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                (cmdSplits.Parameters["date"] as IDbDataParameter).Value = lastSplitDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"); ;
                            }
                            else
                            {
                                (cmd.Parameters["date"] as IDbDataParameter).Value = lastDateTime;
                                (cmdSplits.Parameters["date"] as IDbDataParameter).Value = lastSplitDateTime;
                            }
                            

                            string command = cmd.CommandText;
                            cmd.Prepare();
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                DateTime modDate = DateTime.MinValue;
                                int time = 0, runnerID = 0, iStartTime = 0;
                                string famName = "", fName = "", club = "", classN = "", status = "";
                                
                                try
                                {
                                    //modDate = Convert.ToDateTime(reader[0]);
                                    string sModDate = Convert.ToString(reader[0]);
                                    modDate = ParseDateTime(sModDate);
                                    lastDateTime = (modDate > lastDateTime ? modDate : lastDateTime);
                                    runnerID = Convert.ToInt32(reader["entryid"].ToString());

                                    time = -9;
                                    if (reader["totaltime"] != null && reader["totaltime"] != DBNull.Value)
                                        time = Convert.ToInt32(reader["totalTime"].ToString());

                                    famName = (reader["lastname"] as string);
                                    fName = (reader["firstname"] as string);

                                    //lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                                    club = (reader["clubname"] as string);
                                    classN = (reader["shortname"] as string);
                                    status = reader["runnerStatus"] as string; // reader.GetString(7);

                                    DateTime startTime = DateTime.MinValue;

                                    if (reader["allocatedStartTime"] != null && reader["allocatedStartTime"] != DBNull.Value)
                                    {
                                        string tTime = reader["allocatedStartTime"].ToString();
                                        startTime = ParseDateTime(tTime);
                                    }
                                    if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        string tTime = reader["starttime"].ToString();
                                        startTime = ParseDateTime(tTime);
                                    }


                                    iStartTime = 0;
                                    if (startTime > DateTime.MinValue)
                                    {
                                        iStartTime = (int)(startTime.TimeOfDay.TotalSeconds * 100);
                                    }

                                    if (isRelay)
                                    {
                                        classN = classN + "-" + Convert.ToString(reader["relayLeg"]);
                                        if (reader["finishTime"] != DBNull.Value)
                                        {
                                            DateTime ft = ParseDateTime(reader["finishTime"].ToString());

                                            if (reader["firststarttime"] != DBNull.Value)
                                            {
                                                DateTime ast = ParseDateTime(reader["firststarttime"].ToString());
                                                time = (int)(((TimeSpan)(ft - ast)).TotalSeconds * 100);
                                            }
                                        }
                                    }

                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }


                                /*
                                    time is seconds * 100
                                 * 
                                 * valid status is
                                    notStarted
                                    finishedTimeOk
                                    finishedPunchOk
                                    disqualified
                                    finished
                                    movedUp
                                    walkOver
                                    started
                                    passed
                                    notValid
                                    notActivated
                                    notParticipating
                                 */
                                //EMMAClient.RunnerStatus rstatus = EMMAClient.RunnerStatus.Passed;
                                int rstatus = 0;
                                switch (status)
                                {
                                    case "started":
                                        rstatus = 9;
                                        break;
                                    case "notActivated":
                                        rstatus = 10;
                                        //rstatus = EMMAClient.RunnerStatus.NotStartedYet;
                                        break;
                                    case "notStarted":
                                        rstatus = 1;
                                        //rstatus = EMMAClient.RunnerStatus.NotStarted;
                                        break;
                                    case "disqualified":
                                        rstatus = 4;
                                        break;
                                    case "notValid":
                                        rstatus = 3;
                                        //rstatus = EMMAClient.RunnerStatus.MissingPunch;
                                        break;
                                    case "notParticipating":
                                        rstatus = 999;
                                        break;
                                    case "walkOver":
                                        rstatus = 11;
                                        //rstatus = EMMAClient.RunnerStatus.WalkOver;
                                        break;
                                    case "movedUp":
                                        rstatus = 12;
                                        break;
                                }
                                if (rstatus != 999)
                                {

                                    var res = new Result()
                                        {
                                            ID = runnerID,
                                            RunnerName = fName + " " + famName,
                                            RunnerClub = club,
                                            Class = classN,
                                            StartTime = iStartTime,
                                            Time = time,
                                            Status = rstatus

                                        };
                                    
                                    CheckAndCreatePairRunner(isRelay, reader, runnerPairs, runnerID, res);
                                }
                            }
                            reader.Close();

                            reader = cmdSplits.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    string smod = Convert.ToString(reader[0]);
                                    DateTime mod;
                                    mod = ParseDateTime(smod);

                                    lastSplitDateTime = (mod > lastSplitDateTime ? mod : lastSplitDateTime);

                                    string tTime = Convert.ToString(reader[1]);
                                    DateTime pTime;
                                    pTime = ParseDateTime(tTime);
                                    int sCont = reader.GetInt32(2);
                                    int entryid = Convert.ToInt32(reader["entryid"].ToString());
                                    DateTime startTime;

                                    if (isRelay && reader["firstStartTime"] != null && reader["firstStartTime"] != DBNull.Value)
                                    {
                                        tTime = reader["firstStartTime"].ToString();
                                        startTime = ParseDateTime(tTime);
                                    }
                                    else if (reader["allocatedStartTime"] != null && reader["allocatedStartTime"] != DBNull.Value)
                                    {
                                        tTime = reader["allocatedStartTime"].ToString();
                                        startTime = ParseDateTime(tTime);
                                    }
                                    else if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        tTime = reader["starttime"].ToString();
                                        startTime = ParseDateTime(tTime);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    int passedCount = Convert.ToInt32(reader["passedCount"].ToString());

                                    TimeSpan rTid = pTime - startTime;
                                    double time = rTid.TotalMilliseconds / 10;
                                    List<ResultStruct> times = new List<ResultStruct>();
                                    ResultStruct t = new ResultStruct();
                                    t.ControlCode = sCont + 1000 * passedCount;
                                    t.ControlNo = 0;
                                    t.Time = (int)time;
                                    times.Add(t);

                                    string sfamName = reader["lastname"] as string;
                                    string sfName = reader["firstname"] as string;
                                    string name = (string.IsNullOrEmpty(sfName) ? "" : (sfName + " ")) + sfamName;

                                    string club = reader["clubname"] as string; //.GetString(5);
                                    string classn = reader["shortname"] as string; // reader.GetString(6);

                                    if (isRelay)
                                    {
                                        classn = classn + "-" + Convert.ToString(reader["relayLeg"]);
                                    }

                                    var res = new Result()
                                    {
                                        ID = entryid,
                                        RunnerName = name,
                                        RunnerClub = club,
                                        Class = classn,
                                        StartTime = 0,
                                        Time = -2,
                                        Status = 0,
                                        SplitTimes = times
                                    };

                                    CheckAndCreatePairRunner(isRelay, reader, runnerPairs, entryid, res);
                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }
                            }
                            reader.Close();

                            System.Threading.Thread.Sleep(1000);
                        }
                        catch (Exception ee)
                        {
                            if (reader != null)
                                reader.Close();
                            FireLogMsg("OLA Parser: " + ee.Message + " {parsing: " + lastRunner);

                            System.Threading.Thread.Sleep(100);

                            switch (m_Connection.State)
                            {
                                case ConnectionState.Broken:
                                case ConnectionState.Closed:
                                    m_Connection.Close();
                                    m_Connection.Open();
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    FireLogMsg("OLA Parser: " +ee.Message);
                }
                finally
                {
                    if (m_Connection != null)
                    {
                        m_Connection.Close();
                    }
                    FireLogMsg("Disconnected");
                    FireLogMsg("OLA Monitor thread stopped");

                }
            }
        }

        private void CheckAndCreatePairRunner(bool isRelay, IDataReader reader, Dictionary<int, RunnerPair> runnerPairs, int runnerID, Result res)
        {
            // is this a pair-runner?
            if (!isRelay && reader["allocationControl"] != null && reader["allocationControl"] as string == "groupedWithRef" && reader["allocationEntryId"] != DBNull.Value)
            {
                int otherRunnerId = Convert.ToInt32(reader["allocationEntryId"]);
                RunnerPair rp;

                if (runnerPairs.ContainsKey(runnerID))
                {
                    rp = runnerPairs[runnerID];

                }
                else if (runnerPairs.ContainsKey(otherRunnerId))
                {
                    rp = runnerPairs[otherRunnerId];
                    rp.Runner2 = res;
                    runnerPairs.Add(runnerID, rp);
                }
                else
                {
                    rp = new RunnerPair();
                    rp.Runner1 = res;
                    runnerPairs.Add(runnerID, rp);
                }

                if (rp.Runner1 != null && rp.Runner1.ID == res.ID)
                {
                    rp.Runner1 = res;
                }
                else if (rp.Runner2 != null && rp.Runner2.ID == res.ID)
                {
                    rp.Runner2 = res;
                }

                var comb = rp.CombinedRunner;
                if (comb != null && comb.Status != 999)
                    FireOnResult(comb);

            }
            else
            {
                FireOnResult(res);
            }
        }

        private static DateTime ParseDateTime(string tTime)
        {
            DateTime startTime;
            if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            {
                if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                    {
                        if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                        {
                        }
                    }
                }
            }
            return startTime;
        }
    }
}
