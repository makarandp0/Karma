<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="KarmaCloudService" generation="1" functional="0" release="0" Id="9270aa5d-0479-4099-ae09-17a76da24e68" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="KarmaCloudServiceGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="karmawebapp:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/LB:karmawebapp:Endpoint1" />
          </inToChannel>
        </inPort>
        <inPort name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp">
          <inToChannel>
            <lBChannelMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/LB:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="Certificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/MapCertificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </maps>
        </aCS>
        <aCS name="Certificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/MapCertificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.CertificateThumbprint" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.CertificateThumbprint" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" />
          </maps>
        </aCS>
        <aCS name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </maps>
        </aCS>
        <aCS name="karmawebappInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/MapkarmawebappInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <sFSwitchChannel name="IE:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector">
          <toPorts>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector" />
          </toPorts>
        </sFSwitchChannel>
        <sFSwitchChannel name="IE:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder">
          <toPorts>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder" />
          </toPorts>
        </sFSwitchChannel>
        <lBChannel name="LB:karmawebapp:Endpoint1">
          <toPorts>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Endpoint1" />
          </toPorts>
        </lBChannel>
        <lBChannel name="LB:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput">
          <toPorts>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </toPorts>
        </lBChannel>
        <sFSwitchChannel name="SW:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp">
          <toPorts>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
          </toPorts>
        </sFSwitchChannel>
      </channels>
      <maps>
        <map name="MapCertificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" kind="Identity">
          <certificate>
            <certificateMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
          </certificate>
        </map>
        <map name="MapCertificate|karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" kind="Identity">
          <certificate>
            <certificateMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
          </certificate>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.CertificateThumbprint" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.CertificateThumbprint" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" />
          </setting>
        </map>
        <map name="Mapkarmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" kind="Identity">
          <setting>
            <aCSMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" />
          </setting>
        </map>
        <map name="MapkarmawebappInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebappInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="karmawebapp" generation="1" functional="0" release="0" software="C:\gitspace\karma\server\KarmaCloudService\csx\Release\roles\karmawebapp" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" protocol="tcp" />
              <inPort name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp" portRanges="3389" />
              <outPort name="karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" protocol="tcp">
                <outToChannel>
                  <sFSwitchChannelMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/SW:karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp" />
                </outToChannel>
              </outPort>
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.CertificateThumbprint" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector.Enabled" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;karmawebapp&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;karmawebapp&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteAccess.Rdp&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteDebugger.Connector&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteDebugger.Forwarder&quot; /&gt;&lt;e name=&quot;Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
            <storedcertificates>
              <storedCertificate name="Stored0Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
                </certificate>
              </storedCertificate>
              <storedCertificate name="Stored1Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" certificateStore="My" certificateLocation="System">
                <certificate>
                  <certificateMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp/Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
                </certificate>
              </storedCertificate>
            </storedcertificates>
            <certificates>
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption" />
              <certificate name="Microsoft.WindowsAzure.Plugins.RemoteDebugger.TransportValidation" />
            </certificates>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebappInstances" />
            <sCSPolicyUpdateDomainMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebappUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebappFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="karmawebappUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="karmawebappFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="karmawebappInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="d89744f4-68d9-421f-a3ee-a5d47ffda0e1" ref="Microsoft.RedDog.Contract\ServiceContract\KarmaCloudServiceContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="1c08b6da-d244-4e48-bf28-df8d6d23e50e" ref="Microsoft.RedDog.Contract\Interface\karmawebapp:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp:Endpoint1" />
          </inPort>
        </interfaceReference>
        <interfaceReference Id="ea4dc587-67a0-47ab-900c-35a8bcbccb30" ref="Microsoft.RedDog.Contract\Interface\karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/KarmaCloudService/KarmaCloudServiceGroup/karmawebapp:Microsoft.WindowsAzure.Plugins.RemoteForwarder.RdpInput" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>