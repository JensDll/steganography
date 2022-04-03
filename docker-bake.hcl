variable "TAG" {
  default = ""
}

variable "AWS_ACCOUNT_ID" {
  default = ""
}

variable "REGION" {
  default = "eu-central-1"
}

variable "REPOSITORY" {
  default = "${AWS_ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/image-data-hiding"
}

group "default" {
  targets = [
    "web",
    "web_api"
  ]
}

target "web" {
  context = "services/web"
  tags = [
    "${REPOSITORY}:web.latest",
    notequal("", TAG) ? "${REPOSITORY}:web.${TAG}" : ""
  ]
  platforms = [
    "linux/amd64"
  ]
}

target "web_api" {
  context = "services/api"
  tags = [
    "${REPOSITORY}:web_api.latest",
    notequal("", TAG) ? "${REPOSITORY}:web_api.${TAG}" : ""
  ]
  platforms = [
    "linux/amd64"
  ]
}