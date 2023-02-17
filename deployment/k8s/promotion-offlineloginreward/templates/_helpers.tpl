{{/*
Expand the name of the chart.
*/}}
{{- define "promotion-offlineloginreward.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "promotion-offlineloginreward.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "promotion-offlineloginreward.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "promotion-offlineloginreward.labels" -}}
helm.sh/chart: {{ include "promotion-offlineloginreward.chart" . }}
{{ include "promotion-offlineloginreward.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "promotion-offlineloginreward.selectorLabels" -}}
app.kubernetes.io/name: {{ include "promotion-offlineloginreward.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
tgs.kind: promotion
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "promotion-offlineloginreward.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "promotion-offlineloginreward.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
