variable "DOCKER_BAKE_TAG" {
  default = ""
}

variable "DOCKER_BAKE_REPOSITORY" {
  default = ""
}

variable "DOCKER_BAKE_CONTEXT" {
  default = ""
}

variable "DOCKER_BAKE_PLATFORM" {
  default = ""
}

group "default" {
  targets = [
    "web",
    "api",
  ]
}

target "web" {
  context = "${DOCKER_BAKE_CONTEXT}/web"
  tags = [
    "${DOCKER_BAKE_REPOSITORY}:web.latest",
    notequal("", DOCKER_BAKE_TAG) ? "${DOCKER_BAKE_REPOSITORY}:web.${DOCKER_BAKE_TAG}" : ""
  ]
  platforms = split(",", DOCKER_BAKE_PLATFORM)
}

target "api" {
  context = "${DOCKER_BAKE_CONTEXT}/api"
  tags = [
    "${DOCKER_BAKE_REPOSITORY}:api.latest",
    notequal("", DOCKER_BAKE_TAG) ? "${DOCKER_BAKE_REPOSITORY}:api.${DOCKER_BAKE_TAG}" : ""
  ]
  platforms = split(",", DOCKER_BAKE_PLATFORM)
}
