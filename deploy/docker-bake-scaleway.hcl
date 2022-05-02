variable "TAG" {
  default = ""
}

variable "REPOSITORY" {
  default = "rg.fr-par.scw.cloud/image-data-hiding"
}

group "default" {
  targets = [
    "web",
    "api",
  ]
}

target "web" {
  context = "../services/web"
  tags = [
    "${REPOSITORY}/web:latest",
    notequal("", TAG) ? "${REPOSITORY}/web:${TAG}" : ""
  ]
  platforms = [
    "linux/amd64"
  ]
}

target "api" {
  context = "../services/api"
  tags = [
    "${REPOSITORY}/api:latest",
    notequal("", TAG) ? "${REPOSITORY}/api:${TAG}" : ""
  ]
  platforms = [
    "linux/amd64"
  ]
}
