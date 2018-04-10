<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html"/>

<xsl:template match="/kreive">
<html>
<head>
<title>Apskritiminė kreivė</title>
<style type="text/css">
    body,div,
  dl,dt,dd,
  ul,ol,li,
  h1,h2,h3,h4,h5,h6,
  pre,
  form,fieldset,input,textarea,
  p,blockquote,
  th,td {
  margin:0;
  padding:0;
  }
  table {
  border-collapse:collapse;
  border-spacing:0;
  }
  fieldset,img {
  border:0;
  }
  address,caption,cite,code,dfn,em,strong,th,var {
  font-style:normal;
  font-weight:normal;
  }
  ol,ul {
  list-style:none;
  }
  caption,th {
  text-align:left;
  }
  h1,h2,h3,h4,h5,h6 {
  font-size:100%;
  font-weight:normal;
  }
  q:before,q:after {
  content:'';
  }
  abbr,acronym {
  border:0;
  }

  body {
  
  background-color: #FFFFFF;
  color: #000000;
  font-family:"Trebuchet MS", Arial, Helvetica, sans-serif;
  font-size: 85%;
  }

  h1 {
	  font-size: 1.35em;
	  font-weight: bold;
	  margin-bottom: 1em;
  }
  
  h2 {
	  font-size: 1.25em;
	  font-weight: bold;
	  margin-bottom: 0.5em;
  }
  
  div#container {
  margin: 0.5em;
  padding: 0.5em;
}  
  div {margin-top: 1em; margin-bottom: 1em;}

  a:link, a:visited {color: #0033CC;}
  a:hover {color: #666666;}

  table, th, td {
  vertical-align: middle;
  padding: 2px 5px 2px 5px;
  font-size: 97%;
  }

  table {
    text-align: right;
	margin-bottom: 0.5em;
  }
  
  caption {	
	font-weight: bold;
	text-align: center;
	margin-bottom: 0.3em;
	}

  th {
    font-weight: bold;
    text-align: center;
  }

  td, th {
    border: 1px solid black;
  }

  .wrapword{
    white-space: -moz-pre-wrap !important;  /* Mozilla, since 1999 */
    white-space: -pre-wrap;      /* Opera 4-6 */
    white-space: -o-pre-wrap;    /* Opera 7 */
    white-space: pre-wrap;       /* css-3 */
    word-wrap: break-word;       /* Internet Explorer 5.5+ */
  }

  .align_left {
    text-align: left;
  }

  .align_center {
    text-align: center;
  }

  .align_right {
    text-align: right;
  }
</style>
</head>
<body>
<div id="container">
<h1>Apskritiminė kreivė</h1>
  <div id="parametrai">
  <h2>Kreivės parametrai</h2>
  <table>
    <caption>Pradiniai parametrai</caption>
    <xsl:for-each select="parametrai/pradiniai/parametras">
    <tr>
      <td class="align_left">
        <xsl:value-of select="aprasymas"/>
      </td>
      <td>
        <xsl:value-of select="reiksme"/>
      </td>
    </tr>
    </xsl:for-each> 
  </table>
  <table>
    <caption>Pagrindiniai parametrai</caption>
    <xsl:for-each select="parametrai/pagrindiniai/parametras">
      <tr>
        <td class="align_left">
          <xsl:value-of select="aprasymas"/>
        </td>
        <td>
          <xsl:value-of select="reiksme"/>
        </td>
      </tr>
    </xsl:for-each>   
  </table>
  </div>
  <div id="koordinates">
    <h2>Kreivės taškų koordinatės</h2>
    <table>
      <caption>Sąlyginės koordinatės kreivės pradžios atžvilgiu, mm</caption>      
        <tr>
          <th>taškas</th>
          <th>x</th>
          <th>y</th>
          <th>acad</th>
        </tr>
      <xsl:for-each select="koordinates/salygines/koordinate">
        <tr>
          <td class="align_left">
            <xsl:value-of select="taskas"/>
          </td>
          <td>
            <xsl:value-of select="x"/>
          </td>
          <td>
            <xsl:value-of select="y"/>
          </td>
          <td class="align_center">
            <xsl:value-of select="acad"/>
          </td>
        </tr>
      </xsl:for-each>
    </table>
    <table>
      <caption>Koordinatės kreivės nužymėjimui</caption>
      <tr>
        <th>x, m</th>
        <th>T-x, m</th>
        <th>y, mm</th>
        <th>y realus, mm</th>
        <th>y keisti, mm</th>        
      </tr>
      <xsl:for-each select="koordinates/zymejimo/koordinate">
        <tr>
          <td>
            <xsl:value-of select="x"/>
          </td>
          <td>
            <xsl:value-of select="Tx"/>
          </td>
          <td>
            <xsl:value-of select="y"/>
          </td>
          <td>
            <xsl:value-of select="yreal"/>
          </td>
          <td>
            <xsl:value-of select="ydelta"/>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </div>
</div>
</body>
</html>
</xsl:template>
</xsl:stylesheet>