<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/StudentClubs">
    <html>
      <head>
        <title>Студентські гуртки та семінари</title>
        <style>
          body { font-family: Arial, sans-serif; margin: 24px; color: #222; }
          h2 { margin-bottom: 4px; }
          p.summary { margin-top: 0; color: #555; }
          table { border-collapse: collapse; width: 100%; }
          th, td { border: 1px solid #999; padding: 6px 8px; text-align: left; vertical-align: top; }
          th { background: #e8e4f3; }
          tr:nth-child(even) td { background: #f7f7f7; }
          td.empty { color: #999; }
        </style>
      </head>
      <body>
        <h2>Студентські гуртки та семінари</h2>
        <p class="summary">
          Усього записів: <xsl:value-of select="count(Club)" />
          (гуртків: <xsl:value-of select="count(Club[@Kind = 'гурток'])" />,
          семінарів: <xsl:value-of select="count(Club[@Kind = 'семінар'])" />)
        </p>

        <xsl:choose>
          <xsl:when test="Club">
            <table>
              <tr>
                <th>№</th>
                <th>Назва</th>
                <th>Вид</th>
                <th>Факультет</th>
                <th>Кафедра</th>
                <th>День і час</th>
                <th>Курс</th>
                <th>Тематика</th>
                <th>Староста</th>
                <th>Керівник</th>
              </tr>
              <xsl:for-each select="Club">
                <xsl:sort select="@Faculty" lang="uk" />
                <xsl:sort select="@Name" lang="uk" />
                <xsl:apply-templates select=".">
                  <xsl:with-param name="number" select="position()" />
                </xsl:apply-templates>
              </xsl:for-each>
            </table>
          </xsl:when>
          <xsl:otherwise>
            <p>За заданими параметрами нічого не знайдено.</p>
          </xsl:otherwise>
        </xsl:choose>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Club">
    <xsl:param name="number" />
    <tr>
      <td><xsl:value-of select="$number" /></td>
      <td><b><xsl:value-of select="@Name" /></b></td>
      <td><xsl:value-of select="@Kind" /></td>
      <td><xsl:value-of select="@Faculty" /></td>
      <xsl:choose>
        <xsl:when test="@Department">
          <td><xsl:value-of select="@Department" /></td>
        </xsl:when>
        <xsl:otherwise>
          <td class="empty">—</td>
        </xsl:otherwise>
      </xsl:choose>
      <td><xsl:value-of select="Schedule/@Day" />, <xsl:value-of select="Schedule/@Time" /></td>
      <td><xsl:value-of select="Orientation/@Course" /></td>
      <td><xsl:value-of select="Orientation/@Topic" /></td>
      <td><xsl:value-of select="Head" /></td>
      <td><xsl:value-of select="Supervisor" /></td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
