﻿using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using Kadena2.MicroserviceClients.Clients;
using System;

[assembly: RegisterModule(typeof(Kadena.Old_App_Code.EventHandlers.SettingsKeyEventHandler))]

namespace Kadena.Old_App_Code.EventHandlers
{
    public class SettingsKeyEventHandler : Module
    {
        private const string _workgroupNameSettingKey = "KDA_WorkgroupName";
        private const string _nooshApiSettingKey = "KDA_NooshApi";
        private const string _nooshTokenSettingKey = "KDA_NooshToken";
        private const string _ruleNameSettingKey = "KDA_NooshEventRuleName";
        private const string _rateSettingKey = "KDA_NooshEventRate";
        private const string _targetIdSettingKey = "KDA_NooshEventTargetId";
        private const string _configuratorSettingKey = "KDA_CloudEventConfiguratorUrl";

        public SettingsKeyEventHandler() : base("SettingsKeyEventHandler")
        {
        }

        protected override void OnInit()
        {
            base.OnInit();
            SettingsKeyInfo.TYPEINFO.Events.Insert.After += Update_After;
            SettingsKeyInfo.TYPEINFO.Events.Update.After += Update_After;
            SettingsKeyInfo.TYPEINFO.Events.Delete.After += Update_After;
        }

        private void Update_After(object sender, ObjectEventArgs e)
        {
            if (e.Object.TypeInfo.Equals(SettingsKeyInfo.TYPEINFO))
            {
                var keyName = e.Object.GetStringValue("KeyName", string.Empty);
                switch (keyName)
                {
                    case _workgroupNameSettingKey:
                    case _nooshApiSettingKey:
                    case _nooshTokenSettingKey:
                    case _ruleNameSettingKey:
                    case _rateSettingKey:
                    case _targetIdSettingKey:
                    case _configuratorSettingKey:
                        if (e.Object.Site != null)
                        {
                            UpdateNooshEvent(e.Object.Site as SiteInfo);
                        }
                        else
                        {
                            foreach (var site in SiteInfoProvider.GetSites())
                            {
                                UpdateNooshEvent(site);
                            }
                        }
                        break;
                }
            }
        }

        private void UpdateNooshEvent(SiteInfo site)
        {
            if (site != null)
            {
                var url = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_configuratorSettingKey}");
                var ruleName = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_ruleNameSettingKey}");
                if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(ruleName))
                {
                    var rate = SettingsKeyInfoProvider.GetIntValue($"{site.SiteName}.{_rateSettingKey}");
                    var targetId = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_targetIdSettingKey}");
                    var workGroupName = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_workgroupNameSettingKey}");
                    var nooshUrl = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_nooshApiSettingKey}");
                    var nooshToken = SettingsKeyInfoProvider.GetValue($"{site.SiteName}.{_nooshTokenSettingKey}");

                    bool enabled = rate > 0
                        && !string.IsNullOrWhiteSpace(targetId)
                        && !string.IsNullOrWhiteSpace(workGroupName)
                        && !string.IsNullOrWhiteSpace(nooshToken)
                        && !string.IsNullOrWhiteSpace(nooshUrl);

                    try
                    {
                        var client = new CloudEventConfiguratorClient();
                        var result = client.UpdateNooshRule(url, ruleName, enabled, rate, targetId, workGroupName, nooshUrl, nooshToken).Result;
                        if (!result.Success)
                        {
                            throw new InvalidOperationException(result.ErrorMessages);
                        }
                        else
                        {
                            EventLogProvider.LogInformation("UPDATE - NOOSH EVENT SETTINGS", "MICROREQUEST", result.Payload);
                        }
                    }
                    catch (Exception e)
                    {
                        EventLogProvider.LogException("UPDATE - NOOSH EVENT SETTINGS", "EXCEPTION", e, site.SiteID);
                    }
                }
            }
        }
    }
}