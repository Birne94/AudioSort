<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet version="2.0"
				xmlns:as="http://tempuri.org/DataSet1.xsd"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	
	<xsl:template match="/as:AudiosortDataset" xml:space="default">
		<html>
			<head>
				<title>Audiosort-Datenbank</title>
				<style type="text/css">
					table {
					border-collapse: collapse;
					}
					table, th, td {
					border: 1px solid black;
					}
					td {
					vertical-align: top;
					white-space: nowrap;
					padding: 3px;
					}
					th {
					height: 50px;
					}
					a {
					color: black;
					text-decoration: none;
					}
					a:hover {
					text-decoration: underline;
					}
				</style>
			</head>
			<body>
				<p>
					<a href="#titel">Titel</a>
					<xsl:text> - </xsl:text>
					<a href="#interpreten">Interpreten</a>
          <xsl:text> - </xsl:text>
          <a href="#alben">Alben</a>
          <xsl:text> - </xsl:text>
          <a href="#playlists">Playlisten</a>
        </p>
				<xsl:call-template name="Data"/>
			</body>
		</html>
	</xsl:template>

	<xsl:template name="Data">
		<h1>Titel</h1>
		<table id="titel">
			<tr>
				<th>#</th>
				<th>Name</th>
				<th>Interpret</th>
				<th>Album</th>
				<th>Dauer</th>
				<th>Track</th>
				<th>Pfad</th>
				<th>Genre</th>
				<th>Jahr</th>
				<th>Bewertung</th>
			</tr>
			<xsl:for-each select="as:Titel">
				<xsl:call-template name="Titel"/>
			</xsl:for-each>
		</table>
		<p></p>
		<h1>Interpreten</h1>
		<table id="interpreten">
			<tr>
				<th>#</th>
				<th>Name</th>
			</tr>
			<xsl:for-each select="as:Interpreten">
				<xsl:call-template name="Interpret"/>
			</xsl:for-each>
		</table>
		<p></p>
		<h1>Alben</h1>
		<table id="album">
			<tr>
				<th>#</th>
				<th>Name</th>
				<th>Bild</th>
				<th>Jahr</th>
				<th>Interpret</th>
			</tr>
			<xsl:for-each select="as:Album">
				<xsl:call-template name="Album"/>
			</xsl:for-each>
		</table>
    <p></p>
    <h1>Abspiellisten</h1>
    <table id="playlists">
      <tr>
        <th>#</th>
        <th>Name</th>
        <th>Titel</th>
      </tr>
      <xsl:for-each select="as:Playlist">
        <xsl:call-template name="Playlist"/>
      </xsl:for-each>
    </table>

  </xsl:template>

	<xsl:template name="Titel">
		<xsl:variable name="titel" select="."/>
		<xsl:if test="as:titel_id != -1">
			<tr id="titel_{as:titel_id}">
				<td>
					<xsl:value-of select="as:titel_id"/>
				</td>
				<td>
					<xsl:value-of select="as:titel_name"/>
				</td>
				<td>
					<xsl:for-each select="//as:Interpreten">
						<xsl:if test="as:interpret_id = $titel/as:titel_interpret">
							<a href="#interpret_{as:interpret_id}">
								<xsl:value-of select="as:interpret_name"/>
							</a>
						</xsl:if>
					</xsl:for-each>
				</td>
				<td>
					<xsl:for-each select="//as:Album">
						<xsl:if test="as:album_id = $titel/as:titel_album">
							<a href="#album_{as:album_id}">
								<xsl:value-of select="as:album_name"/>
							</a>
						</xsl:if>
					</xsl:for-each>
				</td>
				<td>
					<xsl:value-of select="as:titel_dauer"/>
				</td>
				<td>
					<xsl:value-of select="as:titel_track"/>
				</td>
				<td>
					<xsl:value-of select="as:titel_pfad"/>
				</td>
				<td>
					<xsl:for-each select="//as:Genre">
						<xsl:if test="as:genre_id = $titel/as:titel_genre">
							<xsl:value-of select="as:genre_name"/>
						</xsl:if>
					</xsl:for-each>
				</td>
				<td>
					<xsl:value-of select="as:titel_jahr"/>
				</td>
				<td>
					<xsl:value-of select="as:titel_bewertung"/>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template name="Interpret">
		<xsl:if test="as:interpret_id != -1">
			<tr id="interpret_{as:interpret_id}">
				<td>
					<xsl:value-of select="as:interpret_id"/>
				</td>
				<td>
					<xsl:value-of select="as:interpret_name"/>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template name="Album">
		<xsl:variable name="album" select="."/>
		<xsl:if test="as:album_id != -1">
			<tr id="album_{as:album_id}">
				<td>
					<xsl:value-of select="as:album_id"/>
				</td>
				<td>
					<xsl:value-of select="as:album_name"/>
				</td>
				<td>
					<xsl:value-of select="as:album_image"/>
				</td>
				<td>
					<xsl:value-of select="as:album_jahr"/>
				</td>
				<td>
					<xsl:for-each select="//as:Interpreten">
						<xsl:if test="as:interpret_id = $album/as:album_interpret">
							<a href="#interpret_{as:interpret_id}">
								<xsl:value-of select="as:interpret_name"/>
							</a>
						</xsl:if>
					</xsl:for-each>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

  <xsl:template name="Playlist">
    <xsl:if test="as:playlist_id != -1">
      <tr id="playlist_{as:playlist_id}">
        <td>
          <xsl:value-of select="as:playlist_id"/>
        </td>
        <td>
          <xsl:value-of select="as:playlist_name"/>
        </td>
        <td>
          <xsl:variable name="playlist" select="."/>
          <xsl:for-each select="//as:PlaylistEntry">
            <xsl:if test="as:playlist_id = $playlist/as:playlist_id">
              <xsl:variable name="title" select="." />
              <xsl:for-each select="//as:Titel">
                <xsl:if test="as:titel_id = $title/as:titel_id">
                  <a href="#titel_{as:titel_id}">
                    <xsl:value-of select="as:titel_name"/>
                  </a>
                  <br />
                </xsl:if>
              </xsl:for-each>
            </xsl:if>
          </xsl:for-each>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>