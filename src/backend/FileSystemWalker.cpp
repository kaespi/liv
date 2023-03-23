#include "FileSystemWalker.h"

#include <QFile>

FileSystemWalker::FileSystemWalker(const QString &file) : m_file(file) {
  if (not file.isEmpty() && QFile::exists(file)) {
    m_file = file;
  }
}

QString FileSystemWalker::getCurrentFile() const { return m_file; }

QString FileSystemWalker::getNextFile() { return m_file; }

QString FileSystemWalker::getPrevFile() { return m_file; }
